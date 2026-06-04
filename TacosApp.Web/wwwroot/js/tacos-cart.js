// ======================================================
// tacos-cart.js  — モーダル初期化・数量調整・カートバッジ更新
// ======================================================

document.addEventListener('DOMContentLoaded', function () {
    // -- トッピングモーダル: 商品情報をフォームにセット --
    const toppingModal = document.getElementById('toppingModal');
    if (toppingModal) {
        toppingModal.addEventListener('show.bs.modal', function (event) {
            const btn = event.relatedTarget;
            if (!btn) return;
            const menuId    = btn.dataset.menuId    || '';
            const menuName  = btn.dataset.menuName  || '';
            const menuPrice = parseFloat(btn.dataset.menuPrice || '0');

            document.getElementById('formMenuId').value    = menuId;
            document.getElementById('formMenuName').value  = menuName;
            document.getElementById('formMenuPrice').value = menuPrice;
            document.getElementById('modalMenuDisplay').textContent = menuName;
            document.getElementById('itemQty').value = '1';

            // チェックボックスを全解除
            document.querySelectorAll('.topping-check').forEach(c => c.checked = false);

            updateSubTotal(menuPrice);
        });

        // トッピング変更 → 小計更新
        toppingModal.addEventListener('change', function () {
            const menuPrice = parseFloat(document.getElementById('formMenuPrice').value || '0');
            updateSubTotal(menuPrice);
        });
    }

    // 数量 +/- ボタン
    const qtyMinus = document.getElementById('qtyMinus');
    const qtyPlus  = document.getElementById('qtyPlus');
    if (qtyMinus) {
        qtyMinus.addEventListener('click', function () {
            const input = document.getElementById('itemQty');
            let v = parseInt(input.value, 10);
            if (v > 1) { input.value = v - 1; input.dispatchEvent(new Event('change')); }
        });
    }
    if (qtyPlus) {
        qtyPlus.addEventListener('click', function () {
            const input = document.getElementById('itemQty');
            let v = parseInt(input.value, 10);
            if (v < 10) { input.value = v + 1; input.dispatchEvent(new Event('change')); }
        });
    }
});

function updateSubTotal(menuPrice) {
    let toppingTotal = 0;
    document.querySelectorAll('.topping-check:checked').forEach(function (c) {
        toppingTotal += parseFloat(c.dataset.price || '0');
    });
    const qty = parseInt(document.getElementById('itemQty')?.value || '1', 10);
    const sub = (menuPrice + toppingTotal) * qty;
    const el = document.getElementById('itemSubTotal');
    if (el) el.textContent = '¥' + sub.toLocaleString('ja-JP');
}

// カート数量バッジ更新（Blazor の StateHasChanged 後にも呼べるよう export）
function updateCartBadge(count) {
    const badge = document.getElementById('navCartCount');
    if (badge) badge.textContent = count;
}

// 数量入力横の +/- ボタン（カートページ）
function adjustQty(btn, delta) {
    const group = btn.closest('.qty-control');
    if (!group) return;
    const input = group.querySelector('input[type=number]');
    let v = parseInt(input.value, 10);
    v = Math.max(1, Math.min(10, v + delta));
    input.value = v;
    input.form && input.form.submit();
}
