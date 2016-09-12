//---------------------------------------------------------------------------------------------------------
// On document ready stuff:
$(function () {

    // Get needed elements from DOM for later use
    $questionText = $("#QuestionText");
    $questionTextValidationMessage = $("#QuestionTextValidationMessage");
    $clientValidationSummary = $(".clientValidationSummary");

    //---------------------------------------------------------------------------------------------------------
    //The following is needed for bootstrap tooltips, see: http://getbootstrap.com/javascript/#tooltips 

    // Trigger tooltips with delay
    $('[data-toggle="tooltip"]').tooltip({
        'delay': { show: 500 },
        trigger: 'hover'
    });
    // Hide tooltip 
    $(document).on('shown.bs.tooltip', function (e) {
        setTimeout(function () { $(e.target).tooltip('hide'); }, 5000);
    });

    //---------------------------------------------------------------------------------------------------------
    // Update visitor stats counter in layout page - use AJAX call to server    
    $.ajax({
        url: '/Admin/Home/GetStats', 
        type: "POST",        
        dataType: 'json',                              
        success: function (data) {            
            var time = data.day + "/" + data.month + "/" + data.year + " " + data.hours + ":" + data.minutes;
            $('#SiteVisitors').text(data.visitorsCount);
            $('#LastAccessTime').text(time);
        },
        error: function (xhr) {
            $('#SiteVisitors').text('Error: Could not get stats');
            $('#LastAccessTime').text('---');
        }
    });

    //---------------------------------------------------------------------------------------------------------
    // Init the tinyMCE editor (Question Text Editor)
    tinymce.init({
        selector: '.editable-tinyMCE-questionText',
        plugins: 'preview link image imagetools table code',
        inline: true,
        setup: function (ed) {
            ed.on('change keyup', function (e) {               
                manageQuestionTextValidationMessage(ed);
            });
        }        
    });

    //---------------------------------------------------------------------------------------------------------
    // Init the tinyMCE editor (Answers Editor)
    tinymce.init({
        selector: '.editable-tinyMCE-answer',
        plugins: 'preview link image imagetools table code',
        inline: true,
        setup: function (ed) {
            ed.on('change keyup', function (e) {
                manageIsCorrectCheckbox(ed);
            });
        }
    });

    //---------------------------------------------------------------------------------------------------------
    // Init the tinyMCE editor (Explantion Editor)
    tinymce.init({
        selector: '.editable-tinyMCE-explanation',
        plugins: 'preview link image imagetools table code',
        inline: true               
    });

    //---------------------------------------------------------------------------------------------------------
    // This is called on tinyMCE Question Text Editor keyup & change events,
    // If no text is present in editor question text area 
    // Then show validation msg
    function manageQuestionTextValidationMessage(editor) {

        if (HasRealContent($questionText.html()) === false) {
            // Alert empty text field validation msg..
            $questionTextValidationMessage.text("The Question Body field is required");
        }
        else {
            // Clear msg..
            $questionTextValidationMessage.html("");
        }
    }

    //---------------------------------------------------------------------------------------------------------
    // This is called on tinyMCE Answers Editory keyup & change events,
    // If no text is present in editor answer text area (in the current tinyMCE that is active)
    // Then disable 'is correct checkbox'
    function manageIsCorrectCheckbox(editor) {

        // 1. Find the checkbox element:
        var $editoryParentDiv = $(editor.bodyElement).parent();
        var $isCorrect = $editoryParentDiv.find(".is-correct-checkbox");

        // 2. Check the content of a the active editor and update the checkbox accordingly:               
        if (HasRealContent(tinyMCE.activeEditor.getContent()) === true)
        {
            $isCorrect.prop('disabled', false);
        }
        else
        {
            $isCorrect.prop('disabled', true);
            $isCorrect.prop('checked', false);
        }
    }

    //---------------------------------------------------------------------------------------------------------
    // Get all answer items in the question page 
    var $answerItems = $(".answer-item");

    //---------------------------------------------------------------------------------------------------------
    // 1. The following is for when posting a new/edited question back to server,
    //    Copy html from the tinyMCE answers div element to the hidden textarea element so that the server sees the answers data.
    //    For some reason this was not needed for the question text & question explanation part..
    //    Maybe because the answers are in a list.. so binding workds differently  
    // 2. Also, Validate tinyMCE input prior to sending ,
    //    the tinyMCE fields need special treatment here, the other fields are taken care by MVC model data annotation validation mechanism   
    
    $(".questionForm").on('submit', function (event) {
        
        // 1. Validate Question Text field is not empty
        if (HasRealContent($questionText.html()) === false) {

            // 1.1 Alert empty text field...
            $questionTextValidationMessage.text("The Question Body field is required");           

            // 1.2 Stop propagation of the submit event
            event.preventDefault();
        }
        else {
            // 2. Loop on answers & copy content to the textarea element that is submitted by the form
            var numberOfAnswers = 0;
            var numberOfIsCorrect = 0;

            for (i = 0; i < $answerItems.length; i++) {

                var divId = '#div_' + i.toString();
                var divContent = $(divId).html();
                var textareaId = "#AnswersList_" + i.toString() + "__AnswerText";

                // 2.0 NOTE: '$(divId).html()' contains the div answer content !
                // BUT: it may have data such as: "<p>&nbsp</p>" which is still considered content and must not be saved to server !
                // So an extra check is needed
                if (HasRealContent(divContent) === true) {

                    // 2.1 Copy content to textarea
                    $(textareaId).val(divContent);

                    // 2.2 Answer has content so increase answers count
                    numberOfAnswers++;

                    // 2.3 Find if isCorrect is checked for this answer
                    var $ParentDiv = $(divId).parent();
                    var $isCorrect = $ParentDiv.find(".is-correct-checkbox");
                    if ($isCorrect.prop("checked")) {
                        numberOfIsCorrect++;
                    }
                }
                else {
                    $(textareaId).val("");
                }
            }

            // 3. Verify we have at least 2 answers
            if (numberOfAnswers < 2) {

                // 3.1 Alert err msg
                $clientValidationSummary.find(".msg").html("<strong>Error:</strong>&nbsp;&nbsp;Minimum of 2 answers is required !");
                $clientValidationSummary.slideDown(600);
                $clientValidationSummary.removeClass("hidden");

                // 3.2 Stop propagation of the submit event
                event.preventDefault();
            }
            else {               

                // 4. Verify we have at least 1 isCorrect checkbox that is checked
                if (numberOfIsCorrect < 1) {

                    // 4.1 Alert err msg
                    $clientValidationSummary.find(".msg").html("<strong>Error:</strong>&nbsp;&nbsp;At least one answer must be marked as 'correct' !");
                    $clientValidationSummary.slideDown(600);
                    $clientValidationSummary.removeClass("hidden");

                    // 4.2 Stop propagation of the submit event
                    event.preventDefault();
                }
                else {
                    // Close/Clear validaton msg                             
                    $clientValidationSummary.slideUp(600);
                }
            }
        }
    });

    //---------------------------------------------------------------------------------------------------------
    // Close/Clear the client side validation bootstrap alert
    $(".my-alert-close").on('click', function () {        
        $clientValidationSummary.slideUp(600);
    });

    //---------------------------------------------------------------------------------------------------------
    // Shorten the question text for the question index list view only    
    $(".QuestionTextInIndexList").each(function () {

        var originalQuestionText = $(this).text();
        var trimmedQuestionText = originalQuestionText.trim();
        var shortTextToShowInIndexList = (trimmedQuestionText.length <= 90) ? trimmedQuestionText :
                                                                              (trimmedQuestionText.substr(0, 90) + "...");
                
        $(this).text(shortTextToShowInIndexList);
    });

});

//---------------------------------------------------------------------------------------------------------
// Parse a string that contains html to see if it has real content 
// i.e. not just line break or any white space
// Return false if string only has br or spaces etc
// Return true if string has real text content
function HasRealContent(inputString) {

    var html = $.parseHTML(inputString);
    var insideText = $(html).text();
    var trimmedText = insideText.trim();

    // Note: The trimmedText variable does not include <img> tags,
    //       Need to take care of that separately, since img is real content..
    var tempString = '<div>' + inputString + '</div>';
    var imageTags = $(tempString).find('img');

    if ((trimmedText === "") && (imageTags.length == 0))
        return false;
    else
        return true;
}

//---------------------------------------------------------------------------------------------------------
// original - BEFORE using tinyMCE
//
//function manageIsCorrectCheckbox ($answerItem)
//{
//    var $answerText = $answerItem.find(".answer-text");
//    var $isCorrect = $answerItem.find(".is-correct-checkbox");
//    if ($answerText.val() != "") {
//        $isCorrect.prop('disabled', false);
//    }
//    else {
//        $isCorrect.prop('disabled', true);
//        $isCorrect.prop('checked', false);
//    }
//}
//---------------------------------------------------------------------------------------------------------