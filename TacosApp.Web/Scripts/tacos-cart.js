/**
 * tacos-cart.js
 * カートへの追加・カートページの数量変更・削除処理
 */
var TacosCart = (function ($) {
    'use strict';

    var _addToCartUrl = '';
    var _cartUrl = '';
    var _removeUrl = '';
    var _updateQtyUrl = '';

    // Anti-Forgery Token をフォームから取得
    function getAntiForgeryToken() {
        return $('input[name="__RequestVerificationToken"]').first().val();
    }

    // ナビゲーションのカートバッジを更新
    function updateNavBadge(count) {
        $('#navCartCount').text(count > 0 ? count : '0');
    }

    // --- メニューページ初期化 ---
    function init(addToCartUrl, cartUrl) {
        _addToCartUrl = addToCartUrl;
        _cartUrl = cartUrl;

        var selectedMenu = {};

        // 「カートに追加」ボタンでモーダルを開く
        $(document).on('click', '.add-to-cart-btn', function () {
            selectedMenu.id = $(this).data('menu-id');
            selectedMenu.name = $(this).data('menu-name');
            selectedMenu.price = parseFloat($(this).data('menu-price'));

            $('#modalMenuName').text(selectedMenu.name + '  ¥' + formatPrice(selectedMenu.price));
            $('#itemQty').val(1);
            $('.topping-check').prop('checked', false);
            updateSubTotal(selectedMenu.price);
        });

        // トッピングチェック変更時に小計を更新
        $(document).on('change', '.topping-check', function () {
            updateSubTotal(selectedMenu.price);
        });

        // 数量 +/- ボタン (モーダル)
        $(document).on('click', '.qty-minus', function () {
            var qty = parseInt($('#itemQty').val());
            if (qty > 1) { $('#itemQty').val(qty - 1); updateSubTotal(selectedMenu.price); }
        });
        $(document).on('click', '.qty-plus', function () {
            var qty = parseInt($('#itemQty').val());
            if (qty < 10) { $('#itemQty').val(qty + 1); updateSubTotal(selectedMenu.price); }
        });
        $(document).on('change', '#itemQty', function () {
            updateSubTotal(selectedMenu.price);
        });

        // モーダルの「カートに追加する」ボタン
        $(document).on('click', '#confirmAddToCart', function () {
            var toppings = [];
            $('.topping-check:checked').each(function () {
                toppings.push({
                    ToppingId: parseInt($(this).val()),
                    Name: $(this).data('name'),
                    Price: parseFloat($(this).data('price'))
                });
            });

            var payload = {
                menuId: selectedMenu.id,
                menuName: selectedMenu.name,
                menuPrice: selectedMenu.price,
                quantity: parseInt($('#itemQty').val()),
                toppingsJson: JSON.stringify(toppings),
                __RequestVerificationToken: getAntiForgeryToken()
            };

            $.post(_addToCartUrl, payload, function (result) {
                if (result.success) {
                    updateNavBadge(result.itemCount);
                    $('#toppingModal').modal('hide');
                    showToast('カートに追加しました！');
                }
            }).fail(function () {
                alert('追加に失敗しました。もう一度お試しください。');
            });
        });
    }

    function updateSubTotal(basePrice) {
        var toppingTotal = 0;
        $('.topping-check:checked').each(function () {
            toppingTotal += parseFloat($(this).data('price'));
        });
        var qty = parseInt($('#itemQty').val()) || 1;
        var sub = (basePrice + toppingTotal) * qty;
        $('#itemSubTotal').text('¥' + formatPrice(sub));
    }

    // --- カートページ初期化 ---
    function initCart(removeUrl, updateQtyUrl) {
        _removeUrl = removeUrl;
        _updateQtyUrl = updateQtyUrl;

        // 削除ボタン
        $(document).on('click', '.remove-item', function () {
            var itemKey = $(this).data('item-key');
            $.post(_removeUrl, {
                itemKey: itemKey,
                __RequestVerificationToken: getAntiForgeryToken()
            }, function (result) {
                if (result.success) {
                    $('[data-item-key="' + itemKey + '"]').remove();
                    updateNavBadge(result.itemCount);
                    refreshCartTotal(result.total);
                    if (result.itemCount === 0) { location.reload(); }
                }
            });
        });

        // 数量 +/- ボタン (カートページ)
        $(document).on('click', '.cart-qty-minus', function () {
            var itemKey = $(this).data('item-key');
            var input = $('.cart-qty-input[data-item-key="' + itemKey + '"]');
            var qty = parseInt(input.val());
            if (qty > 1) {
                input.val(qty - 1);
                postUpdateQty(itemKey, qty - 1);
            }
        });
        $(document).on('click', '.cart-qty-plus', function () {
            var itemKey = $(this).data('item-key');
            var input = $('.cart-qty-input[data-item-key="' + itemKey + '"]');
            var qty = parseInt(input.val());
            if (qty < 10) {
                input.val(qty + 1);
                postUpdateQty(itemKey, qty + 1);
            }
        });
        $(document).on('change', '.cart-qty-input', function () {
            var itemKey = $(this).data('item-key');
            var qty = parseInt($(this).val()) || 1;
            $(this).val(qty);
            postUpdateQty(itemKey, qty);
        });
    }

    function postUpdateQty(itemKey, quantity) {
        $.post(_updateQtyUrl, {
            itemKey: itemKey,
            quantity: quantity,
            __RequestVerificationToken: getAntiForgeryToken()
        }, function (result) {
            if (result.success) {
                updateNavBadge(result.itemCount);
                refreshCartTotal(result.total);
                // ページをリロードして小計を更新
                location.reload();
            }
        });
    }

    function refreshCartTotal(total) {
        var formatted = '¥' + formatPrice(total);
        $('#cartTotal').text(formatted);
        $('#cartGrandTotal').text(formatted);
    }

    function formatPrice(n) {
        return Math.floor(n).toLocaleString('ja-JP');
    }

    function showToast(msg) {
        var toast = $('<div class="taco-toast">' + msg + '</div>');
        $('body').append(toast);
        setTimeout(function () { toast.addClass('show'); }, 100);
        setTimeout(function () { toast.removeClass('show'); setTimeout(function () { toast.remove(); }, 300); }, 2500);
    }

    return { init: init, initCart: initCart };

})(jQuery);
