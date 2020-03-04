// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

function LoadData(target, url, gameTitle, rowNumber, colNumber) {
    $.ajax({
        url: url,
        data: { gameTitle: gameTitle, rowNumber: rowNumber, colNumber: colNumber },
        success: function (result) { $(target).html(result); }
    });
}

$(document).ready(function () {
    $("button").click(function () {
        //console.log(this.id);
        var target = '#' + this.id.toString()
        //console.log("target in function calling:" + target);
        //LoadData(target, 'GetMapImage');
        //console.log("here");
    })
})

function Test(gameTitle, rowNumber, colNumber) {
    console.log(gameTitle + rowNumber + colNumber);
    var target = '#' + rowNumber + colNumber
    console.log("tareget in test:" + target);
    LoadData(target, 'GetMapPiece', gameTitle, rowNumber, colNumber);
}