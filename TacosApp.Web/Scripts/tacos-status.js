/**
 * tacos-status.js
 * SignalR を使った注文ステータスのリアルタイム表示
 * 変数 orderNumber / currentStatus はビュー側で定義される
 */
$(function () {
    'use strict';

    var statusMessages = {
        0: { text: 'ご注文を受け付けました。まもなく調理を開始します。', alertClass: 'alert-info' },
        1: { text: '只今おいしいタコスを調理中です！🌮', alertClass: 'alert-warning' },
        2: { text: '配達中です。もうすぐお届けします！🛵', alertClass: 'alert-primary' },
        3: { text: 'お届けしました！ありがとうございました！🎉', alertClass: 'alert-success' }
    };

    // UI: ステップ表示を更新
    function updateStepUI(status) {
        for (var i = 0; i <= 3; i++) {
            var stepEl = $('#step' + i);
            stepEl.removeClass('active completed');
            if (i < status) {
                stepEl.addClass('completed');
            } else if (i === status) {
                stepEl.addClass('active');
            }
        }

        var info = statusMessages[status] || statusMessages[0];
        $('#statusMessage')
            .removeClass('alert-info alert-warning alert-primary alert-success alert-danger')
            .addClass(info.alertClass);
        $('#statusText').text(info.text);
    }

    // 初期状態を反映
    updateStepUI(currentStatus);

    // SignalR 接続
    var hub = $.connection.orderStatusHub;

    // サーバーからのステータス更新コールバック
    hub.client.statusUpdated = function (status, message) {
        currentStatus = status;
        updateStepUI(status);
    };

    $.connection.hub.start()
        .done(function () {
            $('#connectionStatus').text('接続済み — リアルタイムで更新されます');
            hub.server.joinOrderGroup(orderNumber);
        })
        .fail(function () {
            $('#connectionStatus').text('接続できませんでした。ページを更新してください。');
        });

    // 接続が切れた場合の通知
    $.connection.hub.disconnected(function () {
        $('#connectionStatus').text('接続が切断されました。ページを更新すると再接続できます。');
    });
});
