﻿// ---------- Functions for "CreateMapContent" view ---------- //

// Load buttons for a field, which make available to add text or trial or just show the already added content of it.
function LoadButtonsForAddFieldContent(gameTitle, rowNumber, colNumber) {
    var target = '#ButtonsPlace';

    // Clear the content of the last editing.
    document.getElementById("InputArea").innerHTML = "";
    document.getElementById("fieldDetails").innerHTML = "";
    document.getElementById("ButtonsPlace").innerHTML = "";

    LoadData(target, "GetButtonsForAddFieldContent", gameTitle, rowNumber, colNumber);
}

// Load form for a field to add text content (it contens a text input area).
function LoadFormForAddFieldContent(gameTitle, rowNumber, colNumber) {
    document.getElementById("fieldDetails").innerHTML = "";
    document.getElementById("ErrorMessage").innerHTML = "";
    var target = '#InputArea';
    LoadData(target, "GetFormForFieldContent", gameTitle, rowNumber, colNumber);
}

function LoadFormForAddFieldTrial(gameTitle, rowNumber, colNumber) {
    document.getElementById("fieldDetails").innerHTML = "";
    document.getElementById("ErrorMessage").innerHTML = "";
    var target = '#InputArea';
    LoadData(target, "GetFormForFieldTrial", gameTitle, rowNumber, colNumber);
}

// Save the chosen start field.
function SaveStartField(gameTitle, rowNumber, colNumber) {
    var target = '#InputArea';
    LoadData(target, "SetStartField", gameTitle, rowNumber, colNumber);
}

// Save the chosen target field.
function SaveTargetField(gameTitle, rowNumber, colNumber) {
    var target = '#InputArea';
    LoadData(target, "SetTargetField", gameTitle, rowNumber, colNumber);
}

// If the user clicks on a map field, 
// returns a partial view with the chosen field's data.
function LoadFieldDetails(gameTitle, rowNumber, colNumber) {
    var target = '#fieldDetails';
    document.getElementById("InputArea").innerHTML = "";
    LoadData(target, "GetFieldDetailsPartialView", gameTitle, rowNumber, colNumber);
}



// ---------- Functions for "CreateMap" view ---------- //

// Used at: "CreateMap" view.
// Refreshes the field ("FieldPartialView" partial view) which is changed by the user.
function RefreshField(gameTitle, rowNumber, colNumber) {
    var target = '#' + rowNumber + '-' + colNumber;
    console.log("tareget in test:" + target);
    LoadData(target, 'RefreshField', gameTitle, rowNumber, colNumber);
}

// Used at: "CreateMap" view.
// Send the selected field's code tot the controller, so it will set which type of field the user will fill to the map.
function SetWayDirections(wayDirectionsID, gameTitle) {
    $.ajax({
        url: 'SetRoadID',
        data: { gameTitle: gameTitle, wayDirectionsID: wayDirectionsID }
    });
    console.log(gameTitle + " " + wayDirectionsID);
}

// ---------- Functions for "GameplayView" view ----------//

function Step(gameTitle, rowNumber, colNumber, direction) {
    var target = "#fieldDetails";
    $.ajax({
        url: 'StepGame',
        data: { gameTitle: gameTitle, rowNumber: rowNumber, colNumber: colNumber, direction: direction },
        success: function (result) { $(target).html(result); }
    });
    console.log(direction + " " + gameTitle + " " + rowNumber + " " + colNumber );
}

function ChoseTrialAlternative(gameTitle, rowNumber, colNumber, isAtTargetField) {
    var trialNumber = $('#choseTrialForm').serialize().toString()[6];
    document.getElementById("gameplay-field-text").innerHTML = "";
    LoadTrialResult(gameTitle, rowNumber, colNumber, trialNumber, 'ChoseAlternativeForTrial', "trialForm");
    LoadTrialResult(gameTitle, rowNumber, colNumber, trialNumber, 'GetInformTextAboutTrialResult', "TrialResultInform");
    LoadDirectionButtonsAfterTrial(gameTitle, rowNumber, colNumber, trialNumber, isAtTargetField,
        'LoadDirectionButtonsAfterTrial', "#direction-buttons");
    setTimeout(function () { RefreshLifeCountAfterTrial(gameTitle, 'GetLifeCount', "#life-count"); }, 100);
    
}

function Teleport(gameTitle, rowNumber, colNumber) {
    var target = "#fieldDetails";
    LoadData(target, 'DoTeleport', gameTitle, rowNumber, colNumber);
}

// Helper function: loads  the trial result by ajax call.
function LoadTrialResult(gameTitle, rowNumber, colNumber, trialNumber, url, target) {
    $.ajax({
        url: url,
        data: {
            gameTitle: gameTitle, rowNumber: rowNumber, colNumber: colNumber, trialNumber: trialNumber
        },
        success: function (result) { document.getElementById(target).innerHTML = result; }
    });
}

function LoadDirectionButtonsAfterTrial(gameTitle, rowNumber, colNumber, trialNumber, isAtTargetField, url, target) {
    $.ajax({
        url: url,
        data: {
            gameTitle: gameTitle, rowNumber: rowNumber, colNumber: colNumber,
            trialNumber: trialNumber, isAtTargetField: isAtTargetField
        },
        success: function (result) { $(target).html(result); }
    });
}

function RefreshLifeCountAfterTrial(gameTitle, url, target) {
    console.log("refreshing");
    $.ajax({
        url: url,
        data: { gameTitle: gameTitle },
        success: function (result) { $(target).html(result); }
    })
}

// ---------- Functions for Index of GameViewer controller ----------//

function LoadDetails(gameTitle) {
    var target = "#index-content";
    console.log(gameTitle);
    $.ajax({
        url: '../GameViewer/GameViewerDetails',
        data: { gameTitle: gameTitle },
        success: function (result) { $(target).html(result); }
    });
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

