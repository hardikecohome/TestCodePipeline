function pmt(rate_per_period, number_of_payments, present_value, future_value, type) {
    if (rate_per_period != 0.0) {
        var q = Math.pow(1 + rate_per_period, number_of_payments);
        return -(rate_per_period * (future_value + (q * present_value))) / ((-1 + q) * (1 + rate_per_period * (type)));

    } else if (number_of_payments != 0.0) {
        return -(future_value + present_value) / number_of_payments;
    }

    return 0;
}

function pv(rate, nper, pmt, fv) {

    nper = parseFloat(nper);

    pmt = parseFloat(pmt);

    fv = parseFloat(fv);

    rate = parseFloat(rate);;

    if ((nper == 0)) {

        return (0);

    }

    if (rate == 0) {

        pv_value = -(fv + (pmt * nper));

    }

    else {

        x = Math.pow(1 + rate, -nper);

        y = Math.pow(1 + rate, nper);

        pv_value = -(x * (fv * rate - pmt + y * pmt)) / rate;

    }

    pv_value = conv_number(pv_value, 2);

    return (pv_value);

}

function conv_number(expr, decplaces) {
    var str = "" + Math.round(eval(expr) * Math.pow(10, decplaces));

    while (str.length <= decplaces) {

        str = "0" + str;

    }

    var decpoint = str.length - decplaces;

    return (str.substring(0, decpoint) + "." + str.substring(decpoint, str.length));

}