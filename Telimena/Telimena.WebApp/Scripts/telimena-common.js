function HideTopAlertBox() {
    var $box = getTopAlertBox();
    $box.hide();
}

function showSuccessTopAlert(text, header) {
    if (!header) {
        header = "Success";
    }
    showTopAlertBox('callout-success', 'icon fa fa-check', text, header);
}

function showSuccessTopAlertAndRefresh(text, header) {
    if (!header) {
        header = "Success";
    }
    showTopAlertBox('callout-success', 'icon fa fa-check', text + ". Reloading page...", header);
    setTimeout(location.reload.bind(location), 1500);

}

function showWarningTopAlert(text, header) {
    if (!header) {
        header = "Warning";
    }
    showTopAlertBox('callout-warning', 'icon fa fa-warning', text, header);
}

function showDangerTopAlert(text, header) {
    if (!header) {
        header = "Error";
    }
    showTopAlertBox('callout-danger', 'icon fa fa-ban', text, header);
}

function getXhrErrorMessage(xhr) {
    var text = "Something went wrong";
    if (xhr) {
        if (xhr.responseJSON && xhr.responseJSON.Message) {
            return xhr.responseJSON.Message;
        } else if (xhr.statusText) {
            return xhr.statusText;
        }
    }
    return text;
}


function showInfoTopAlert(text, header) {
    if (!header) {
        header = "Info";
    }
    showTopAlertBox('callout-info', 'icon fa fa-info', text, header);
}


function showTopAlertBox(alertClass, iconClass, text, header) {
    var $box = getTopAlertBox();
    $box.find('h4').text(header);
    $box.find('div.text').text(text);
    $box.find('i').css(iconClass);
    $box.show().attr("class", "alert callout alert-dismissible " + alertClass);
}

function getTopAlertBox() {
    return $($('#TopAlertBox')[0]);    
}

function getTopAlertBoxTextElement($box) {
    if (!$box) {
        $box = getTopAlertBox();
    } 
    return $box.find('div.text');
}




$(function () {$("[data-hide]").on("click", function () {
        $(this).closest("." + $(this).attr("data-hide")).hide();
    });
});


function renderSequenceHistoryUrl(data, type, url) {
    if (type === 'display') {
                        
        data = '<a  href="' + url + '" data-toggle="tooltip" data-placement="top" title="Click to see what happened before and after for this user">' + data + '&nbsp;<i class="fa fa-share-square-o"></i></a>';
    }
    return data;
}

function convertUTCDateToLocalDate(date) {
    //this is not needed anymore
    return date;
}

Number.prototype.padLeft = function (base, chr) {
    var len = (String(base || 10).length - String(this).length) + 1;
    return len > 0 ? new Array(len).join(chr || '0') + this : this;
}

function globalUtcConversion() {
    $('.utcdate-to-localdate').each(function () {
        try {
            $(this).html(toFormattedTimestamp($(this).text()));
        }
        catch (ex) {
            //do nothing
        }
    });
}

function toDdMmYyyy(date) {
    var datePart = date.split(" ")[0];
    var timePart = date.split(" ")[1];
    var fromTime = timePart.split(":");

    var fromDate = datePart.split(/\.|\//);
    return new Date(fromDate[2], fromDate[1] - 1, fromDate[0], fromTime[0], fromTime[1], fromTime[2]);
}

function isValidDate(d) {
    return d instanceof Date && !isNaN(d);
}

function toFormattedTimestamp(utcDate) {
    if (utcDate === '' || utcDate === undefined) {
        return '';
    }
    if (!isValidDate(utcDate)) {
        utcDate = toDdMmYyyy(utcDate);
    }
    var d = convertUTCDateToLocalDate(utcDate);
    var formatted = [
        d.getDate().padLeft(),
        (d.getMonth() + 1).padLeft(),
        d.getFullYear()].join('/') + ' ' +
        [d.getHours().padLeft(),
            d.getMinutes().padLeft(),
        d.getSeconds().padLeft()].join(':');
    return formatted;
}