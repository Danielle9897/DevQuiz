//---------------------------------------------------------------------------------------------------------
// Global variables
var currentQuizSessionId = 0; // A simple Id given to the sub-category quiz session that was started by the user, 
                              // so that ajax results/next question info are published only if user has not switched in the middle 
                              // and started another quiz in another sub-category

var totalQuestionsNumber;
var currentQuestionNumber; // The question sequential number as in the db, not the question id..
var currentQuestionId;     // The question Id, as in the db
var visibleQuestionNumber; // The visuall question number that shows to user (can be different than the one from db..)

var selectedSubCategoryId;    // The sub-category Id as in the database
var selectedSubCategoryIndex; // The sub-category index, the index in the Dom array 

var $selectedLevel = "1";         // Novice
var $selectedTimeLimit = "false"; // NoTimeLimit
var clock;                        // A variable used by setinverval & clearinterval methods, if time limit is on
var showLoaderTimeout;            // Used for delaying time when showing loader

//---------------------------------------------------------------------------------------------------------
// On document ready stuff:
$(function () {

    //---------------------------------------------------------------------------------------------------------
    // Cache elements (that are used more than once) from DOM, for later multi use 
    $questionArea       = $('#QuestionArea');
    $startQuizButton    = $('#StartQuizButton');
    $submitAnswerButton = $('#SubmitAnswerButton');
    $quizResultsArea    = $('#QuizResultsArea');
    $questionPanelArea  = $('#QuestionPanelArea');
    $dropItem           = $('.drop-item');
    $explanationArea    = $('#ExplanationArea');
    $nextQuestionButton = $('#NextQuestionButton');
    $timeElement        = $('#TimeElement');
    $subCategoriesList  = $('.selectedSubCategory');
    $selectLevelOption  = $('#SelectLevelOption');

    //---------------------------------------------------------------------------------------------------------
    // Dropdown choice was selected - Update the header value of the dropdown choice (LEVEL & TIME options..)
    $(".dropdown-menu li a").on('click', function () {
        $(this).parents(".dropdown").find('.selectedItem').html($(this).text() + ' <span class="caret"></span>');
    });
    $("#SelectLevelOption .dropdown-menu li a").on('click', function () {
        $selectedLevel = $(this).attr('data-level-value');
    });
    $("#SelectTimeOption .dropdown-menu li a").on('click', function () {
        $selectedTimeLimit = $(this).attr('data-time-value');
    });

    //---------------------------------------------------------------------------------------------------------
    // Quiz SUBJECT was selected 
    $subCategoriesList.on('click', function () {
       
        // 1. Update the name of chosen subcategory selected & set params       
        $('#SubCategoryChosen').html($(this).data('sub-category-name'));
        selectedSubCategoryId = $(this).data('sub-category-id');

        selectedSubCategoryIndex = $subCategoriesList.index(this);

        // 2. Show start button & set visibility of other elements
        $('#StartMessage').addClass('hidden');
        $questionArea.addClass('hidden');
        $startQuizButton.removeClass('hidden');
        $submitAnswerButton.attr({ disabled: true });
        $quizResultsArea.addClass('hidden');
        $questionPanelArea.removeClass('hidden');
        $dropItem.removeClass('disabled')
        LoaderOff();

        // 3. Enable/Disable the options in question level drop down according to number of questions            
        var LevelNoviceQuestionsNumber = ($subCategoriesList.eq(selectedSubCategoryIndex)).data('sub-category-number-of-questions-level-novice');
        var LevelIntermediateQuestionsNumber = ($subCategoriesList.eq(selectedSubCategoryIndex)).data('sub-category-number-of-questions-level-intermediate');
        var LevelExpertQuestionsNumber = ($subCategoriesList.eq(selectedSubCategoryIndex)).data('sub-category-number-of-questions-level-expert');

        if (LevelExpertQuestionsNumber == 0) {
            $("#Expert").addClass('disabled');
        }
        if (LevelIntermediateQuestionsNumber == 0) {
            $("#Intermediate").addClass('disabled');           
        }
        if (LevelNoviceQuestionsNumber == 0) {
            $("#Novice").addClass('disabled');            
        }

        // 3.1 Update Level Header accordingly        
        $selectLevelOption.find('.selectedItem').html("Expert" + ' <span class="caret"></span>');
        $selectedLevel = "3";
        if (LevelNoviceQuestionsNumber > 0) {
            $selectLevelOption.find('.selectedItem').html("Novice" + ' <span class="caret"></span>');
            $selectedLevel = "1";
        }
        else if (LevelIntermediateQuestionsNumber > 0) {
            $selectLevelOption.find('.selectedItem').html("Intermediate" + ' <span class="caret"></span>');
            $selectedLevel = "2";
        }        

        // 4. Clear local storage and set relevant data
        var quizLocalStorage = window.localStorage;
        quizLocalStorage.clear();
        quizLocalStorage.selectedSubCategoryId = selectedSubCategoryId;       

        // 5. Clear clock          
        StopTimer();

        // If a previous test was already in progress then notify user ?
    });

    //---------------------------------------------------------------------------------------------------------
    // Quiz START Button was clicked
    $startQuizButton.on('click', function () {

        // 1. Set quiz session id
        //    So that if user switches to another category in the middle of a quiz session
        //    The ajax result that comes back will not be treated..
        currentQuizSessionId = (currentQuizSessionId > 100) ? 0 : (currentQuizSessionId+1)

        // 2. Set elements visiblity 
        $startQuizButton.addClass('hidden');
        $submitAnswerButton.attr({ disabled: true });
        $dropItem.addClass('disabled');

        // 3. Initialize so we will get the first question when doing the ajax request 
        currentQuestionNumber = 0; 
        visibleQuestionNumber = 0;     

        // 4. Set the total number of questions relevant for this quiz session according to the selected level
        var numberOfQuestionsAttribute;

        if ($selectedLevel === "1") {
            // Novice  
            numberOfQuestionsAttribute = 'sub-category-number-of-questions-level-novice';
        }
        else if ($selectedLevel === "2") {
            // Intermediate           
            numberOfQuestionsAttribute = 'sub-category-number-of-questions-level-intermediate';
        }
        else {
            // Expert          
            numberOfQuestionsAttribute = 'sub-category-number-of-questions-level-expert';
        }

        totalQuestionsNumber = ($subCategoriesList.eq(selectedSubCategoryIndex)).data(numberOfQuestionsAttribute);

        // 5. Call the ajax request to get the first question data for the selected sub-category
        //    Show Loader if slow connection..        
        LoaderOn();
        GetNextQuestion();          
    });

    //---------------------------------------------------------------------------------------------------------
    // SUBMIT ANSWER button was clicked
    $submitAnswerButton.on('click', function () {

        // 1. Get answers list
        var $answersList = $("#QuestionArea ul li");

        // 1.1 Get the list of all answers that user has checked (the "li" element)      
        var $selectedAnswers = $answersList.find("input:checked").closest("li"); // works both for radio & checkboxes..

        // 1.2 Get the list of all answers marked as 'correct'    
        var $correctAnswers = $answersList.filter(function () {
             return $(this).data("iscorrectAnswer") == true
        });

        // 2. Save current question answers data to the browser LOCAL WEB STORAGE        
        // 2.1 First build the question jason object                 
       
        var questionKeyString = "QuestionNumber" + visibleQuestionNumber;
        var questionDataString = currentQuestionId;

        if ($selectedAnswers.length == 0) {
            // User selected 0 answers
            questionDataString += (",0");
        }
        else {
            // User selected at least one answer
            $selectedAnswers.each(function () {
                questionDataString += ("," + $(this).data("answerId"));
            });
        }
       
        // 2.2 Save to local storage
        window.localStorage.setItem(questionKeyString, questionDataString);

        // 3. If NoTimeLimit than show results for this question && show 'next question' btn 
        if ($selectedTimeLimit === "false") {

            // 3.1 Color all correct answers Green
            $correctAnswers.each(function (index) {
                $(this).addClass('correct-answer');
            });

            // 3.2 Color the wrong answers that were selected Red                    
            var $wrongAnswersSelected = $selectedAnswers.filter(function () {
                   return $(this).data("iscorrectAnswer") == false
            });
            $wrongAnswersSelected.each(function (index) {
                  $(this).addClass('wrong-answer');
            });

            // 3.3 Show Explanation Area if Explenation is not null..
            var explenation = $questionArea.data("Explanation");
            if (explenation != null) {                  

                $explanationArea.find('#ExplanationText')
                                .html(explenation)
                                .end()
                                .css({ "display": "none" })
                                .removeClass('hidden')
                                .slideDown(250);
            }

            // 3.4 Manage buttons 
            $submitAnswerButton.addClass('hidden');
            $nextQuestionButton.removeClass('hidden').text("Next Question");

            // 3.5 Check if this was the last question
            if (visibleQuestionNumber == totalQuestionsNumber) {
                $nextQuestionButton.html("Finish");
            }                              

            // 3.6 Don't show border upon hover on answers + make input disabled          
            $answersList.each(function (index) {
                $(this).on('mouseenter', function() { $(this).removeClass('answer-hover') 
                       .find("input").prop('disabled', true).end()
                       .find("label").css("cursor", "default")
                })
            });
        }

        // 4. With TIME LIMIT - Get next question from server using ajax call
        else 
        {
            // 4.1 Stop the timer
            StopTimer();

            // 4.2 If that was the last question then send quiz result to server 
            if (visibleQuestionNumber == totalQuestionsNumber)
            {
                SendQuizDataToServer()
            }
            else
            {                
                GetNextQuestion();
            }
        } 
    });

    //---------------------------------------------------------------------------------------------------------
    // NEXT Question / FINISH was clicked
    $nextQuestionButton.on('click', function ()
    {
        // 0. Disable button & Show Loader if slow connection..
        $nextQuestionButton.attr({ disabled: true });

        $questionArea.addClass('hidden');
        LoaderOn();

        // 1. If that was the last question then send quiz results to server
        if (visibleQuestionNumber == totalQuestionsNumber) {                                  
            
            // 1.1 Trigger an Ajax call that returns a result view
            SendQuizDataToServer();

            // 1.2 Enable choosing level again for the next quiz
            $dropItem.removeClass('disabled');
        }
        else {
            // 2. Else, get the next question            
            GetNextQuestion();
        }
    });

    //---------------------------------------------------------------------------------------------------------
    // Handle main menu arrows icons
    $('.collapse').on('shown.bs.collapse', function () {        
                       $(this).prev().find('.glyphicon-triangle-bottom')
                                     .removeClass('glyphicon-triangle-bottom')
                                     .addClass('glyphicon-triangle-top');
                   })
                  .on('hidden.bs.collapse', function () {                            
                      $(this).prev().find('.glyphicon-triangle-top')
                                     .removeClass('glyphicon-triangle-top')
                                     .addClass('glyphicon-triangle-bottom');
                  });

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

}); // End on document ready stuff

//---------------------------------------------------------------------------------------------------------
// Make an AJAX request to get next Question
function GetNextQuestion() {

    var jsonObject = {
        "currentQuizSessionId": currentQuizSessionId,
        "subCategoryId" : selectedSubCategoryId,
        "questionNumber": currentQuestionNumber,
        "QuestionLevel" : $selectedLevel
    };

    $.ajax({        
        url: 'Quiz/GetNextQuestion',
        type: "POST",
        contentType: 'application/json; charset=utf-8', // outgoing data type
        data: JSON.stringify(jsonObject),               // the outgoing data
        processData: true,
        dataType: 'json',                               // expected returned data type
        success: function (data) {
            // Update view view with the next question info only if the results that came back from the ajax call 
            // are for the current quiz sub-category session
            // i.e. if user has switched to another quiz session than no point in showing the info   
            if (currentQuizSessionId === data.currentQuizSessionId) {
                
                CreateQuestionElements(data.nextQuestionView);
                LoaderOff();
                $questionArea.css({ "display": "none" });
                $questionArea.removeClass('hidden');
                $questionArea.fadeIn(1000);
            }                           
        }
        //error: function (xhr) { no need for this really..
        //    alert('ajax is back with: !error!');           
        //}
    });
};
//---------------------------------------------------------------------------------------------------------
// Update the DOM Question Area with the json info returned from the ajax call (The next question info)
function CreateQuestionElements(data) {

    if (data === "NoMoreQuestions")
    {  
        throw "Error: No more questions recieved";
    }
    else
    {
        currentQuestionId = data.QuestionId;
        currentQuestionNumber = data.QuestionNumber;
        visibleQuestionNumber++;       
        $('#QuestionNumber').text(visibleQuestionNumber);
        $('#TotalNumberOfQuestions').text(totalQuestionsNumber);
        //$("#QuestionText").text(data.QuestionText);
        $('#QuestionText').html(data.QuestionText);

        // 0. Clear previous answers first
        $('#AnswersArea').children().remove();

        // 1. Save meta data on the question element
        $questionArea.data({
                "CreditPoints": data.CreditPoints,
                "QuestionId":   data.QuestionId,
                "SingleAnswer": data.SingleAnswer,
                "Explanation":  data.Explanation,
        });       

        // 2. Loop on all answers
        for (i = 0; i < data.AnswersList.length; i++) { 

            var answerType = (data.SingleAnswer === true) ? "radio" : "checkbox";
            var answerElement = "<li><div><label><input/><span></span></label></div></li>";

            // 3. Create the answer element and append to DOM
            $(answerElement).addClass("well well-sm answer-well")                            
                            .data({
                                "iscorrectAnswer": data.AnswersList[i].IsCorrect,  // meta data.. 
                                "answerId": data.AnswersList[i].AnswerId
                            })                                                     
                            .on('mouseenter', function () { $(this).addClass('answer-hover') })
                            .on('mouseleave', function () { $(this).removeClass('answer-hover') })
                            .on('click', function (e) { AnswerClicked(this, e); })                            
                                    .find("input").attr({ type: answerType, name: 'inputGroup', id: 'Id' + i.toString() })
                                                  .end()
                                    .find("div").addClass(answerType)
                                                .end()
                                    .find("label").attr({ for: 'Id' + i.toString() })                                                 
                                                  .end()
                                    .find("span").html(data.AnswersList[i].AnswerText)
                                                 .end()
                            .appendTo("#AnswersArea");
        }

        // 3. Manage buttons (disable 'submit answer btn' untill user selects an answer..)
        $submitAnswerButton.attr({ disabled: true })
                           .removeClass('hidden');
      
        $nextQuestionButton.attr({ disabled: false })
                           .addClass('hidden');

        $explanationArea.addClass('hidden');

        // 4. Start timer if needed
        if ($selectedTimeLimit === "true") {
            if (data.TimeToAnswer == 0) {
                // User chose to run w/ Time Limit BUT question has 0 seconds from the admin db..
                $timeElement.text("Question has no time limit..").removeClass('hidden');
            }
            else {
                // User chose to run w/ Time Lime AND question has time in seconds (> 0) from the admin db...
                StartTimer(data.TimeToAnswer, $timeElement);
            }
        }
    }    
};

//---------------------------------------------------------------------------------------------------------
// ANSWER area was clicked
function AnswerClicked(answerItem, eventInfo) {

     // 1. Stop event propagation from outer div to the inner input element (otherwise it bubbles back up resulting in a loop)
     eventInfo.stopImmediatePropagation();

     // 2. eventInfo.target is the element that user clicked on..    
     //    Explicitly trigger click event on the inner input element if coming form the outer div
     if ((!$(eventInfo.target).is('input')) && (!$(eventInfo.target).is('label')))
     {
         $(answerItem).find('input').trigger('click');             
     }

     // 3. Enable the 'submit answer' btn if at least one answer was checked
     var numberOfSelectedAnswers = $("#QuestionArea ul li").find("input:checked").length;
     if (numberOfSelectedAnswers >= 1) {
         $submitAnswerButton.attr({ disabled: false })
     }
     else {
         $submitAnswerButton.attr({ disabled: true });
     }
};

//---------------------------------------------------------------------------------------------------------
// Send the quiz answers that were saved in the local storage to the server - using AJAX
function SendQuizDataToServer() {

    // 1. Build a json object to be sent to server
    var jsonObjToSend = {
        currentQuizSessionId: currentQuizSessionId,
        SubCategoryId: window.localStorage.getItem("selectedSubCategoryId"),
        QuestionsData: []
    }

    // 2. Retrieve the quiz data from local browser web storage and fill the jsonObjToSend object 
    for (i = 1; i <= totalQuestionsNumber; i++) {
        var questionAnswersFromStorage = window.localStorage.getItem("QuestionNumber" + i.toString()).split(",");
        var oneQuestionData = {
            QuestionId: questionAnswersFromStorage[0], // The first place is the question Id
            AnswersSelected: []                        // The rest is the answers Ids
        }

        for (j = 1; j < questionAnswersFromStorage.length; j++) {
            oneQuestionData.AnswersSelected.push({ "AnswerId": questionAnswersFromStorage[j] });
        }

        jsonObjToSend.QuestionsData.push(oneQuestionData);
    }

    // 3. Send info by ajax
    $.ajax({
        url: 'Quiz/ProcessResults',
        type: "POST",
        contentType: 'application/json; charset=utf-8',  // outgoing data type
        data: JSON.stringify(jsonObjToSend),             // the outgoing data  
        dataType: 'html',                                // expected returned data type
        success: function (data) {

            // 3.1 Update view only if the results that came back from the ajax call are 
            // for the current quiz sub-category session
            // i.e. if user has switched to another quiz session than no point in showing the results
            var ajaxQuizSessionId = parseInt(($(data).filter("#quizSessionId")).val());
            if (currentQuizSessionId == ajaxQuizSessionId) {

                $quizResultsArea.html(data);
                LoaderOff();
                $questionPanelArea.addClass('hidden');

                $quizResultsArea.css({ "display": "none" })
                                .removeClass('hidden')
                                .fadeIn(1000)
            }
        }
        //error: function (xhr) {
        //    alert('ajax is back from ProcessResults controller with error --- ' + xhr.statusText);
        //}
    });
};

//---------------------------------------------------------------------------------------------------------
// A timer function showing the countdown clock, every second
function StartTimer(duration, display) {

    $timeElement.removeClass('hidden');

    var time = duration;
    var minutes, seconds;

    function displayTime() {
        minutes = parseInt(time / 60, 10);
        seconds = parseInt(time % 60, 10);

        minutes = minutes < 10 ? "0" + minutes : minutes;
        seconds = seconds < 10 ? "0" + seconds : seconds;

        display.text(minutes + ":" + seconds);        

        if (--time < 0) {

            // 1. Time is up ! Stop current timer 
            StopTimer();

            // 2. Continue by immitating 'Submit Answer Button' was pressed    
            $submitAnswerButton.trigger("click");
        }
    };

    displayTime();

    clock = setInterval(displayTime, 1000);   
};

//---------------------------------------------------------------------------------------------------------
// Stop the countdown clock
function StopTimer() {
    clearInterval(clock);
    $timeElement.addClass('hidden');
};

//---------------------------------------------------------------------------------------------------------
// Show Loader Modal
function LoaderOn() {
  
    //$body = $('body');
    //$loadermodal = $('.loader-modal');
    //$areaToHide = $('#QuestionPanelArea');
    //var pos = $areaToHide.offset();

    //$loadermodal.height($areaToHide.height())
    //            .width($areaToHide.width())
    //            .css({ top: pos.top })
    //            .css({ left: pos.left });    

    //$body.addClass("loading");

    showLoaderTimeout = setTimeout("$('#Loader').removeClass('hidden')", 900);    
};

//---------------------------------------------------------------------------------------------------------
// Remove Loader Modal
function LoaderOff() {
    //$("body").removeClass("loading");

    clearTimeout(showLoaderTimeout);
    $('#Loader').addClass('hidden');
}

