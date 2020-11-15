"use strict";
//console.dir("start");// as state

window.onload = Init();
localStorage.setItem("storyIndex", "1");

function Init() {
    var btn = document.getElementById("bSubmit");
    if (btn != null) {
        btn.addEventListener("click", LoadPartial, false);
    }

    InitCanvas();
    InitTree();
}

function LoadPartial() {
    //console.dir($("#enteredURL").val());
    //$("#treeview").load("/Home/CreateMap", { url: $("#enteredURL").val() });
    sessionStorage.url = JSON.stringify({ url: $("#enteredURL").val() });
}

function Confirn() {
    jQuery.extend({
        postJSON: function (data) {
            return jQuery.ajax({
                type: "POST",
                url: "/JSONRequests/GetURL",
                data: JSON.stringify({ 'URL': data }),
                Error: "err(x)",
                dataType: "json",
                contentType: "application/json",
                processData: false
            });
        }
    });
    $.postJSON($("#enteredURL").val());
    //$.postJSON(createJSON()[0]);
}

function createJSON() {
    var JSON_obj = [];
    var url = $("#enteredURL").val();
        var item = {};
    item["URL"] = url;
    //console.dir(url);

        JSON_obj.push(item);
    console.dir(JSON_obj);
    return JSON_obj;
}


function InitCanvas() {
    var res = document.getElementById("hiddenDataPoints").innerText.split("/n");
    //console.dir(res);
    var dps = [
        { x: 0, y: 0 }
    ];
    var index = 0;
    for (var i = 0; i < res.length-1; i++) {
        //console.dir(res[i].indexOf('Speed:'));
        //console.dir(res[i].substring(res[i].indexOf('Speed:') + 6));
        //console.dir(parseFloat(res[i].substring(res[i].indexOf('Speed:') + 6)));
        dps.push({ x: index++, y: parseFloat(res[i].substring(res[i].indexOf('Speed:') + 6)) });
    }
    
    //console.dir(dps);
  

    var chart = new CanvasJS.Chart("chartContainer1", {
        title: {
            text: "Data points (ms)",
            fontFamily: "Verdana"
        },
        subtitles: [{
            text: "0 it's your last request",
            fontFamily: "Verdana"
        }],
        data: [{
            type: "splineArea",
            dataPoints: dps
        }]
    });
    chart.render();

    $("a.canvasjs-chart-credit").css(" ");
    $("a.canvasjs-chart-credit").hide();

}

function InitTree() {
    //$("#partial").load("/Home/CreateMap", { url: JSON.parse(sessionStorage.url).url });
    //console.dir($("#childCount").val());
    $.ajax({
        type: "GET", url: "/Home/CreateMap",
        //data: JSON.stringify({ 'url': JSON.parse(sessionStorage.url).url, 'linkCountForParent':t}),
        data: { url: JSON.parse(sessionStorage.url).url, linkCountForParent: $("#childCount").val() },
        success: function (data) {
            $('#partial').html(data);
        }
    });
}
