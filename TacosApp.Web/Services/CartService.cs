using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using TacosApp.Web.Models.ViewModels;

namespace TacosApp.Web.Services
{
    /// <summary>セッションベースのカートサービス</summary>
    public class CartService
    {
        private const string SessionKey = "TacosCart";
        private readonly ISession _session;

        public CartService(IHttpContextAccessor httpContextAccessor)
        {
            _session = httpContextAccessor.HttpContext!.Session;
        }

        public CartViewModel GetCart()
        {
            string? json = _session.GetString(SessionKey);
            if (string.IsNullOrEmpty(json))
            {
                return new CartViewModel();
            }

            CartViewModel? cart = JsonConvert.DeserializeObject<CartViewModel>(json);
            if (cart == null)
            {
                return new CartViewModel();
            }
            return cart;
        }

        public void AddItem(CartItemViewModel newItem)
        {
            CartViewModel cart = GetCart();
            CartItemViewModel existing = null;
            foreach (CartItemViewModel i in cart.Items)
            {
                if (i.ItemKey == newItem.ItemKey)
                {
                    existing = i;
                    break;
                }
            }

            if (existing != null)
            {
                existing.Quantity += newItem.Quantity;
            }
            else
            {
                cart.Items.Add(newItem);
            }
            Save(cart);
        }

        public void RemoveItem(string itemKey)
        {
            CartViewModel cart = GetCart();
            CartItemViewModel item = FindByKey(cart, itemKey);
            if (item != null)
            {
                cart.Items.Remove(item);
                Save(cart);
            }
        }

        public void UpdateQuantity(string itemKey, int quantity)
        {
            CartViewModel cart = GetCart();
            CartItemViewModel item = FindByKey(cart, itemKey);
            if (item != null)
            {
                if (quantity <= 0)
                {
                    cart.Items.Remove(item);
                }
                else
                {
                    item.Quantity = quantity;
                }
                Save(cart);
            }
        }

        public void Clear()
        {
            _session.Remove(SessionKey);
        }

        public decimal GetTotal()
        {
            return GetCart().Total;
        }

        private static CartItemViewModel FindByKey(CartViewModel cart, string itemKey)
        {
            foreach (CartItemViewModel i in cart.Items)
            {
                if (i.ItemKey == itemKey)
                {
                    return i;
                }
            }
            return null;
        }

        private void Save(CartViewModel cart)
        {
            _session.SetString(SessionKey, JsonConvert.SerializeObject(cart));
        }
    }
}
