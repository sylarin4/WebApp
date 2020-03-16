// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.


// ---------- Functions for "CreateMapContent" view ---------- //


// Used at: "CreateMapContent" view.
// Load a form ("FormForAddFieldContent" partial view) to the "CreateMapContent" view after the user selected the field
// he / she wants to add text or trial.
function LoadFormForAddFieldContent(gameTitle, rowNumber, colNumber) {
    var target = '#InputArea';
    LoadData(target, 'GetFormForField', gameTitle, rowNumber, colNumber);
}

function AddNewAlternativeForForm(index, gameTitle, rowNumber, colNumber) {
    var target = '#alternative' + index;
    console.log(target);
    console.log(index);
    $.ajax({
        url: "GetNewAlternative",
        data: { index: index, gameTitle: gameTitle, rowNumber: rowNumber, colNumber: colNumber },
        success: function (result) { $(target).html(result); }
    });
    $.ajax({
        url: "RefreshAddAlternativeButton",
        data: { index: index, gameTitle: gameTitle, rowNumber: rowNumber, colNumber: colNumber },
        success: function (result) { $("#addAlternativeButton").html(result); }
    });
}

// ---------- Functions for "CreateMap" view ---------- //

// Used at: "CreateMap" view.
// Refreshes the field ("FieldPartialView" partial view) which is changed by the user.
function RefreshField(gameTitle, rowNumber, colNumber) {
    //console.log(gameTitle + rowNumber + colNumber);
    var target = '#' + rowNumber + colNumber
    //console.log("tareget in test:" + target);
    LoadData(target, 'RefreshField', gameTitle, rowNumber, colNumber);
}

// Used at: "CreateMap" view.
// Send the selected field's code tot the controller, so it will set which type of field the user will fill to the map.
function SetWayDirections(wayDirectionsID, gameTitle) {
    $.ajax({
        url: 'SetRoadID',
        data: { gameTitle: gameTitle, wayDirectionsID: wayDirectionsID }
    });
    //console.log(gameTitle + " " + wayDirectionsID);
}


// ---------- Helper functions ---------- //


// A helper function for ajax sending. Used in several js functions.
function LoadData(target, url, gameTitle, rowNumber, colNumber) {
    $.ajax({
        url: url,
        data: { gameTitle: gameTitle, rowNumber: rowNumber, colNumber: colNumber },
        success: function (result) { $(target).html(result); }
    });
}


// ---------- Currently not used functions ---------- //
// (Maybe some of them will be needed or can be useful for something.)


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


// Now it's not used, because .serialize() method messes up special hungarien characters like "ő", so it doesn't work well now.
// If possible, we should fix this problem and use this method for posting field content, so we don't have to reload 
// the whole page for it.
function SaveTextContent(gameTitle, rowNumber, colNumber) {
    var textContent = $('#textContentForm').serialize().toString();
    textContent = textContent.substring(12, textContent.lenght);
    console.log(textContent);
    $.ajax({
        url: 'SaveTextContent',
        data: { gameTitle: gameTitle, rowNumber: rowNumber, colNumber: colNumber, textContent: textContent }
    });
}