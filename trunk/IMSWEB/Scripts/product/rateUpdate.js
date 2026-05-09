$(document).on("click", "#showmodalbtn", function () {
    var ProductID = $(this).data("productid");
    var salesRate = $(this).data("salesrate");
    var purchaseRate = $(this).data("purchase");
    var productname = $(this).data("productname");
    $("#SalesRate").val(salesRate);
    $("#PurchaseRate").val(purchaseRate);
    $("#ProductID").val(ProductID);
    $("#ProductName").val(productname);
    $("#myModal").modal("show");
    //alert(ProductID);
});


$(document).on("click", "#Submitbtn", function () {

    var salesRate = $("#SalesRate").val();
    var purchaseRate = $("#PurchaseRate").val();

    var ProductID = getDefaultIntIfEmpty($("#ProductID").val());

    if ((salesRate == "" || salesRate == 0)) {
        toastr.error("Please Enter Sales Rate.");
        return;
    }
    if ((purchaseRate == "" || purchaseRate == 0)) {
        toastr.error("Please Enter Purchase Rate.");
        return;
    }

    var SalesRateModel = {
        'ProductID': ProductID,
        'SalesRate': salesRate,
        'PurchaseRate': purchaseRate
    };

    $.ajax({
        url: "/Product/UpdateRate/",
        type: "POST",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        data: JSON.stringify(SalesRateModel),
        success: function (data) {
            //console.log(data);
            if (data == true) {
                $("#myModal").modal("hide");
                ClearTextBox();
            }
            location.reload();
        },
        error: function (data) {
            console.log(data);
            toastr.error("Update Failed!");
            location.reload();
        }

    });

});

function ClearTextBox() {
    $("#ProductID").val("");
    $("#ProductName").val("");
    $("#SalesRate").val("");
    $("#PurchaseRate").val("");
}