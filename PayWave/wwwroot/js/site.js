// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

$(function () {

    $('input[name="saveTokenCheckbox"]').change(function () {
        var address = $('#Form_Address').val();
        var alias = $('#Form_Alias').val();
        var prefix = $('#Form_Prefix').val();
        if (alias != '' && prefix != undefined && prefix != '') {
            address = prefix + alias.toLowerCase();
            if ($('input[name="saveTokenCheckbox"]:checked').val() == "true") {
                
                saveToken(address, $('#Form_Token').val());
            }
            else {
                localStorage.removeItem(address);
            }
        }
        else {
            if (address != '') {
                if ($('input[name="saveTokenCheckbox"]:checked').val() == "true") {
                    saveToken($('#Form_Address').val(), $('#Form_Token').val());
                }
                else {
                    localStorage.removeItem($('#Form_Address').val());
                }
            }
        }
        

    });

    var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'))
    var tooltipList = tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl)
    })

    $('.checkBoxShowToken').change(function () {
        $(this).is(':checked') ? $('.tokenField').attr('type', 'text') : $('.tokenField').attr('type', 'password');
    });

    $('.tokenField_change').change(function () {
        localStorage.setItem($('#Form_Address').val(), $('#Form_Token').val());
    });

    $('.dateTimeLocalFormat').each(function () {
        var dateValue = $(this).text();
        var localDate = convertUtcToLocalDateTime(dateValue);
        $(this).text(localDate);
    });

    $('.balanceWallet').each(function () {
        var addressAccount = $(this).data('addressaccount');
        var urlBalance = $(this).data('urlbalance');
        var tokenAccount = localStorage.getItem(addressAccount);
        if (tokenAccount != '' && tokenAccount != null) {
            fetch(urlBalance, {
                method: "GET",
                headers: { "Content-type": "application/json;charset=UTF-8", "Authorization": "Bearer " + tokenAccount }
            })
                .then(response => response.json())
                .then(json =>
                    $(this).text((json.accountBalance.netBalance / Math.pow(10, json.assetScale)).toFixed(2) + " " + json.assetCode)
                )
                .catch(err => console.log(err));
        }
    });

    $('.balanceTestXRPLWallet').each(async function () {
        var addressAccount = $(this).data('addressaccount');
        var urlBalance = $(this).data('urlbalance');
        const client = new xrpl.Client(urlBalance);
        await client.connect();
        try {
            const accountbalance = (await client.getXrpBalance(addressAccount));
            $(this).text(accountbalance);
        } catch { }
        client.disconnect();
    });

    $('.balanceCBDCXRPLWallet').each(function () {
        var walletId = $(this).data('walletid');
        fetch('/Wallet/GetNativeBalanceCBDC/' + walletId, {
            method: "GET",
            headers: { "Content-type": "application/json;charset=UTF-8" }
        })
            .then(response => response.json())
            .then(json =>
                $(this).text(json.valueAsXrp)
            )
            .catch(err => console.log(err));
    });

    $('.loadingButton').on('click', function () { $("#loadingoverlay").fadeIn(); });

    $('.copyClipboardButton').on('click', function () {
        var copyText = $(".textToClipboard").val();
        /* Copy the text inside the text field */
        navigator.clipboard.writeText(copyText);
        $('.textLinkCopied').css('display', 'block');
    });

    $(".sendformImage").click(function () {
        $(".sendForm").submit();
    });

});

function fillTokenFromLocalStorage(address) {
    if (localStorage.getItem(address) != null) {
        $('.tokenField').val(localStorage.getItem(address));
        
    }
}

function convertUtcToLocalDateTime(datetimeValue) {
    var dateString = datetimeValue + " UTC";
    var theDate = new Date(Date.parse(dateString));
    return theDate.toLocaleString();
}

function saveToken(address, value) {
    localStorage.setItem(address, value);
}

async function getXRPLBalances(account, network) {
    const client = new xrpl.Client(network);
    await client.connect()
    const balances = await client.getBalances(account)
    client.disconnect()
    if (balances != null && balances.length > 0) {
        balances.forEach((moneda, indice) => {
            $('.balanceCurrenciesXRPL').append('<tr><td>' + moneda.currency + '</td><td>' + moneda.value + '</td><td>' + moneda.issuer + '</td></tr>');
        });
    }
}

async function sendXRPLPayment(originAddress,network,assetCode,amount,destinationAddress) {
    try {
        const originWallet = xrpl.Wallet.fromSeed($('#Form_Token').val());
        if (originWallet == null || originWallet.address != originAddress) {
            $('.loadingNewPaymentDiv').css('display', 'none');
            $('.errorData').css('display', 'block');
        }
        else {
            $('.loadingNewPaymentDiv').css('display', 'block');
            $('.errorData').css('display', 'none');
            const client = new xrpl.Client(network);
            await client.connect()
            if (assetCode == 'XRP') {
                const prepared = await client.autofill({
                    "TransactionType": "Payment",
                    "Account": originWallet.address,
                    "Amount": xrpl.xrpToDrops(amount),
                    "Destination": destinationAddress
            });
            const signed = originWallet.sign(prepared);
            const tx = await client.submitAndWait(signed.tx_blob);
            if (tx.result != null && tx.result.meta != null && tx.result.meta.TransactionResult != null
                && tx.result.meta.TransactionResult == "tesSUCCESS") {
                $('#Form_successfulPayment').val(true);
                $('#Form_amountDelivered').val(tx.result.meta.delivered_amount);
                $('#Form_amountSent').val(tx.result.Amount);
                $('#Form_originalAmount').val(tx.result.Amount);
            }
            else {
                $('#Form_Confirmed').val(false);
            }
            $('#secretCodeForm').submit();
        }
                        else {
            var found_paths = await client.request({
                "command": "ripple_path_find",
                "source_account": originAddress,
                "destination_account": destinationAddress,
                "destination_amount": {
                    "value": amount,
                    "currency": assetCode,
                    "issuer": destinationAddress
                }
            });
            if (found_paths != null && found_paths.result != null && found_paths.result.status == 'success') {
                if (found_paths.result.alternatives != null && found_paths.result.alternatives.length > 0) {
                    const prepared = await client.autofill({
                        "TransactionType": "Payment",
                        "Account": originWallet.address,
                        "Paths": found_paths.result.alternatives,
                        "Amount": {
                            "currency": assetCode,
                            "value": amount,
                            "issuer": destinationAddress
                        },
                        "Destination": destinationAddress
                    });
                    const signed = originWallet.sign(prepared);
                    const tx = await client.submitAndWait(signed.tx_blob);
                    if (tx.result != null && tx.result.meta != null && tx.result.meta.TransactionResult != null
                        && tx.result.meta.TransactionResult == "tesSUCCESS") {
                        $('#Form_successfulPayment').val(true);
                        $('#Form_amountDelivered').val(tx.result.meta.delivered_amount);
                        $('#Form_amountSent').val(tx.result.Amount);
                        $('#Form_originalAmount').val(tx.result.Amount);
                    }
                    else {
                        $('#Form_Confirmed').val(false);
                    }
                    $('#secretCodeForm').submit();
                }
                else {
                    $('.loadingNewPaymentDiv').css('display', 'none');
                    $('.noLiquidityErrorData').css('display', 'block');
                }
            }
            else {
                $('.loadingNewPaymentDiv').css('display', 'none');
                $('.errorData').css('display', 'block');
            }
        }

        client.disconnect();
        }
    }
                catch (err) {
            $('.loadingNewPaymentDiv').css('display', 'none');
            $('.errorData').css('display', 'block');
        }
}


async function sendDeferredPayment(originAddress, network, assetCode, amount, destinationAddress) {
    try {
        const originWallet = xrpl.Wallet.fromSeed($('#Form_Token').val());
        if (originWallet == null || originWallet.address != originAddress) {
            $('.loadingNewPaymentDiv').css('display', 'none');
            $('.paymentSuccessDiv').css('display', 'none');
            $('.errorData').css('display', 'block');
        }
        else {
            $('.loadingNewPaymentDiv').css('display', 'block');
            $('.errorData').css('display', 'none');
            const client = new xrpl.Client(network);
            await client.connect()
            const prepared = await client.autofill({
                "TransactionType": "CheckCreate",
                "Account": originWallet.address,
                "SendMax": assetCode == 'XRP' ? xrpl.xrpToDrops(amount) : {
                    "currency": assetCode,
                    "value": amount,
                    "issuer": destinationAddress
                },
                "Destination": destinationAddress
            });
            const signed = originWallet.sign(prepared);
            const tx = await client.submitAndWait(signed.tx_blob);
            if (tx.result != null && tx.result.meta != null && tx.result.meta.TransactionResult != null
                && tx.result.meta.TransactionResult == "tesSUCCESS") {
                var checkCreated = tx.result.meta.AffectedNodes.filter(x => x.CreatedNode != null && x.CreatedNode.LedgerEntryType == 'Check');
                if (checkCreated.length == 1) {
                    $('#Form_amountDelivered').val(assetCode == 'XRP' ? xrpl.xrpToDrops(amount): amount);
                    $('#Form_amountSent').val(assetCode == 'XRP' ? xrpl.xrpToDrops(amount) : amount);
                    $('#Form_originalAmount').val(assetCode == 'XRP' ? xrpl.xrpToDrops(amount) : amount);
                    $('#Form_successfulPayment').val(true);
                    $('#Form_ExternalId').val(checkCreated[0].CreatedNode.LedgerIndex);
                    $('.loadingNewPaymentDiv').css('display', 'none');
                    $('.paymentSuccessDiv').css('display', 'block');
                    $('#deferredPaymentForm').submit();
                }
                else {
                    $('#Form_Confirmed').val(false);
                }
                

                
            }
            else {
                $('#Form_Confirmed').val(false);
            }
            
            
            client.disconnect();
        }
    }
    catch (err) {
        $('.loadingNewPaymentDiv').css('display', 'none');
        $('.errorData').css('display', 'block');
    }
}



async function cashMoneyFromPayment(originAddress, network, assetCode, amount, checkId) {
    try {
        const originWallet = xrpl.Wallet.fromSeed($('#Form_Token').val());
        if (originWallet == null || originWallet.address != originAddress) {
            $('.loadingNewPaymentDiv').css('display', 'none');
            $('.paymentSuccessDiv').css('display', 'none');
            $('.errorData').css('display', 'block');
        }
        else {
            $('.loadingNewPaymentDiv').css('display', 'block');
            $('.errorData').css('display', 'none');
            const client = new xrpl.Client(network);
            await client.connect()
            const prepared = await client.autofill({
                "TransactionType": "CheckCash",
                "Account": originWallet.address,
                "Amount": assetCode == 'XRP' ? xrpl.xrpToDrops(amount) : {
                    "currency": assetCode,
                    "value": amount,
                    "issuer": originWallet.address
                },
                "CheckID": checkId
            });
            const signed = originWallet.sign(prepared);
            const tx = await client.submitAndWait(signed.tx_blob);
            if (tx.result != null && tx.result.meta != null && tx.result.meta.TransactionResult != null
                && tx.result.meta.TransactionResult == "tesSUCCESS" && tx.result.validated == true) {
                    $('.loadingNewPaymentDiv').css('display', 'none');
                    $('.paymentSuccessDiv').css('display', 'block');
                    $('#Form_Cashed').val(true);
                    $('#cashPaymentForm').submit();
            }
            else {
                $('#Form_Cashed').val(false);
            }


            client.disconnect();
        }
    }
    catch (err) {
        $('.loadingNewPaymentDiv').css('display', 'none');
        $('.errorData').css('display', 'block');
    }
}