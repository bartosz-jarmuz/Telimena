function convertUTCDateToLocalDate(date) {
    var newDate = new Date(date.getTime() + date.getTimezoneOffset() * 60 * 1000);

    var offset = date.getTimezoneOffset() / 60;
    var hours = date.getHours();

    newDate.setHours(hours - offset);

    return newDate;
}

Number.prototype.padLeft = function (base, chr) {
    var len = (String(base || 10).length - String(this).length) + 1;
    return len > 0 ? new Array(len).join(chr || '0') + this : this;
}

function globalUtcConversion() {
    $('.utcdate-tolocaldate').each(function () {
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

    var fromDate = datePart.split(".");
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