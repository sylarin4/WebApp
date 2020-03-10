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

// This is not needed now. If stay useless, delete.
//$(document).ready(function () {
  //  $("button").click(function () {
        //console.log(this.id);
        // var target = '#' + this.id.toString()
        //console.log("target in function calling:" + target);
        //LoadData(target, 'GetMapImage');
        //console.log("here");
    //})
//})

function SaveTextContent(gameTitle, rowNumber, colNumber) {
    var textContent = $('#textContentForm').serialize().toString();
    textContent = textContent.substring(12, textContent.lenght);
    console.log(textContent);
    $.ajax({
        url: 'SaveTextContent',
        data: { gameTitle: gameTitle, rowNumber: rowNumber, colNumber: colNumber, textContent: textContent }
    });
}

function LoadTextInputFieldForMapPiece(gameTitle, rowNumber, colNumber) {
    var target = '#InputArea';
    LoadData(target, 'GetInputAreaForMapPiece', gameTitle, rowNumber, colNumber);
}

function RefreshMapPiece(gameTitle, rowNumber, colNumber) {
    //console.log(gameTitle + rowNumber + colNumber);
    var target = '#' + rowNumber + colNumber
    //console.log("tareget in test:" + target);
    LoadData(target, 'GetMapPiece', gameTitle, rowNumber, colNumber);
}



function SendWayDirectionsData(url, gameTitle, wayDirectionsID) {
    $.ajax({
        url: url,
        data: { gameTitle: gameTitle, wayDirectionsID: wayDirectionsID }
    });
}

function SetWayDirections(wayDirectionsID, gameTitle) {
    SendWayDirectionsData('SetRoadID', gameTitle, wayDirectionsID);
    console.log(gameTitle + " " + wayDirectionsID);
}

