(function ($) {

    /**************************
    *A function to show View Notification
    ***************************/
    this.showViewNotification = function (result, message) {
        var goodUiStateClass = 'ui-state-highlight';
        var badUiStateClass = 'ui-state-error';
        var goodUiNotificationIconClass = 'ui-icon-info';
        var badUiNotificationIconClass = 'ui-icon-alert';

        if (result === undefined || message === undefined || typeof (result) !== 'boolean' || typeof (message) !== 'string')
            throw new Error("Invalid arguments provided");

        if (result) {
            $('#view_notification_ui_state').removeClass(badUiStateClass);
            $('#view_notification_icon').removeClass(badUiNotificationIconClass);
            $('#view_notification_ui_state').addClass(goodUiStateClass);
            $('#view_notification_icon').addClass(goodUiNotificationIconClass);
            $('#view_notification_state').text("Information: ");
            $('#view_notification_message').text(message);
        }
        else {
            $('#view_notification_ui_state').removeClass(goodUiStateClass);
            $('#view_notification_icon').removeClass(goodUiNotificationIconClass);
            $('#view_notification_ui_state').addClass(badUiStateClass);
            $('#view_notification_icon').addClass(badUiNotificationIconClass);
            $('#view_notification_state').text("Error: ");
            $('#view_notification_message').text(message);
        }
        $('#view_notification').stop(true, true);
        $('#view_notification').slideDown(500).delay(3000).slideUp(1000);
    };

    /**************************
    *A function to delete a row in a table
    ***************************/
    this.deleteRow = function (deleteUrl, itemID, itemType, itemName) {
        $("#dialog-confirm-item-type").text(itemType);
        $("#dialog-confirm-item-name").text(itemName);
        $(function () {
            $("#dialog-confirm").dialog({
                resizable: false,
                height: 'auto',
                width: 'auto',
                modal: true,
                buttons: {
                    Delete: function () {
                        $.post(deleteUrl, { 'id': itemID }, function (responsejson, status) {
                            if (status == 'success') {
                                if (responsejson.Result)
                                    $('tr.item_row' + itemID).remove();
                                showViewNotification(responsejson.Result, responsejson.Message);
                            }
                            else {
                                showViewNotification(false, 'There was some network related error, please try again');
                            }

                        }, 'json');
                        $(this).dialog("close");
                    },
                    Cancel: function () {
                        $(this).dialog("close");
                    }
                }
            });
        });
    };

    this.setTableStatus = function (tableid, status) {
        if (arguments[0] instanceof RestaurantTable) {
            var args = arguments[0];
            tableid = args.TableId;
            status = args.Status;
        }
        $('img#table_status' + tableid).attr('src', function () {
            if (status === RestaurantTable.RestaurentTableStatus.Booked) {
                $(this).attr('data-status', 'Booked');
                return TableStatusIMG.Booked;
            }
            if (status === RestaurantTable.RestaurentTableStatus.Occupied) {
                $(this).attr('data-status', 'Occupied');
                return TableStatusIMG.Occupied;
            }
            if (status === RestaurantTable.RestaurentTableStatus.Vacant) {
                $(this).attr('data-status', 'Vacant');
                return TableStatusIMG.Vacant;
            }
            if (status === TableStatusIMG.BeingBooked) {
                $(this).attr('data-status', 'Selected');
                return TableStatusIMG.BeingBooked;
            }
            return undefined;
        });
    };

})(jQuery);