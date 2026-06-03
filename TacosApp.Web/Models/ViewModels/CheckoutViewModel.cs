using System.ComponentModel.DataAnnotations;

namespace TacosApp.Web.Models.ViewModels
{
    public class CheckoutViewModel
    {
        [Required(ErrorMessage = "お名前を入力してください")]
        [StringLength(100, ErrorMessage = "お名前は100文字以内で入力してください")]
        [Display(Name = "お名前")]
        public string CustomerName { get; set; }

        [Required(ErrorMessage = "電話番号を入力してください")]
        [StringLength(20, ErrorMessage = "電話番号は20文字以内で入力してください")]
        [RegularExpression(@"^[0-9\-\+\(\) ]+$", ErrorMessage = "電話番号の形式が正しくありません")]
        [Display(Name = "電話番号")]
        public string Phone { get; set; }

        [Required(ErrorMessage = "配達先住所を入力してください")]
        [StringLength(500, ErrorMessage = "住所は500文字以内で入力してください")]
        [Display(Name = "配達先住所")]
        public string DeliveryAddress { get; set; }

        [StringLength(500, ErrorMessage = "備考は500文字以内で入力してください")]
        [Display(Name = "備考・配達メモ")]
        public string DeliveryNote { get; set; }
    }
}
