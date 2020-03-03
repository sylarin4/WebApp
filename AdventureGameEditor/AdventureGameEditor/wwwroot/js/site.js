// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

function LoadData(target, url) {
    $.ajax({
        url: url,
        success: function (result) { $(target).html(result); }
    });
}

$(document).ready(function () {
    $("button").click(function () {
        console.log(this.id);
        var target = '#' + this.id.toString()
        console.log(target);
        //$("ajax").html('<img src="@Url.Action("GetMapImage","HomeController",new { id = 1  })" />')
        LoadData(target, 'GetMapImage');
        console.log("here");
    })
})