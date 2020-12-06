"use strict";
//console.dir("start");// as state

window.onload = Init();
localStorage.setItem("storyIndex", "1");
var chart2;
var timerId;

function Init() {
    var btn = document.getElementById("bSubmit");
    if (btn != null) {
        btn.addEventListener("click", LoadPartial, false);
    }

    InitCanvas();
    InitTree();
    RegisterModal();
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

    chart2 = new CanvasJS.Chart("chartContainer2", {
        title: {
            text: "Data points",
            fontFamily: "Verdana"
        },
        subtitles: [{
            text: "(ms)",
            fontFamily: "Verdana"
        }],    
        axisX: {
            valueFormatString: "#",
            interval: 1,
            labelFontSize: 0
        },
        data: [{
            type: "splineArea",
            dataPoints: [{ x: 0, y: 0 }]
        }]
    });
    chart2.render();
}

function InitTree() {
    //$("#partial").load("/Home/CreateMap", { url: JSON.parse(sessionStorage.url).url });
    //console.dir($("#childCount").val());
    if (sessionStorage.url != null) {
        $.ajax({
            type: "GET", url: "/Home/CreateMap",
            //data: JSON.stringify({ 'url': JSON.parse(sessionStorage.url).url, 'linkCountForParent':t}),
            data: { url: JSON.parse(sessionStorage.url).url, linkCountForParent: $("#childCount").val() },
            success: function (data) {
                $('#partial').html(data);
                clearInterval(timerId);
            }
        });
    }
}


function RegisterModal() {
    var modal = document.getElementById("Modal");
    var span = document.getElementsByClassName("closeModal")[0];
    var showModal = document.querySelectorAll('.showModal');
    //console.dir(showModal);
    showModal[0].addEventListener('click', function () {
        modal.style.display = "block";
    });

    span.onclick = function () {
        modal.style.display = "none"; 
    };
    window.onclick = function (event) {
        if (event.target == modal) {
            modal.style.display = "none";
        }
    };

    timerId = setInterval(Refresh, 1000);
    setTimeout(() => { clearInterval(timerId); }, 180000);

}

function Refresh() {
    $.ajax({
        type: "GET", url: "/Home/Refresh",
        success: function (data) {
            $('#modalPartial').html(data);
        }
    });
  

    //console.dir(chart2.options);
    
    $.ajax({
        type: "GET", url: "/Home/RefreshChart",
        success: function (data) {
            //console.dir(data.split("</div>"));
            for (var i = 0; i < data.split("</div>").length-1; i++) {
                var reg = new RegExp('<b class="ping">(.*)</b>.*<span class="title">(.*)</span>');
                var res = data.split("</div>")[i].match(reg);
                //console.dir(" [ "+i+" ] "+res[0]);
                //console.dir(" [ " + i + " ] " +res[1].match(/\d+,\d+/)[0]);
                //console.dir(" [ " + i + " ] " + res[2]);

                if (chart2.options.data[0].dataPoints[chart2.options.data[0].dataPoints.length - 1].label === res[2]) {
                    continue;
                }

                chart2.options.data[0].dataPoints.push({
                    x: chart2.options.data[0].dataPoints.length, y: Number(res[1].match(/\d+,\d+/)[0].replace(',', '.')),
                    indexLabel: "" + chart2.options.data[0].dataPoints.length, label: "" + res[2]
                });
            }
           
            chart2.render();
            $("a.canvasjs-chart-credit").css(" ");
            $("a.canvasjs-chart-credit").hide();
        }
    });
}

function OK() {
    document.getElementById("Modal").style.display = "none";
}
