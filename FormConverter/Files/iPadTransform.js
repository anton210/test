var sinaturePadApi = null;
var currentSignature = null;
var currentSignatureWidth = null;
var signatureDialog = null;

var signature = null;

$(document).ready(function () {

  var signature = new Signature();
  signature.initSignature(document.getElementsByClassName('sigWrapper')[0]);
  signature.setupCanvas();

  signatureDialog = $('#popup');

  $('#sigPadClearButton').click(function () {
    signature.clearSignature();
  });

  $('#sigPadDoneButton').click(function () {
    var output = $(currentSignature).find('input.signature');
    output.attr('value', signature.getSignature());
    var image = $(currentSignature).find('img.preview');
    image.attr('src', signature.getSignature());
    $('#popup').dialog('close');
  });

  $('.signLink').click(function () {
    currentSignature = this;
    var jsonSign = $(currentSignature).find('input.signature').attr('value');

    signature.clearSignature();
    signature.setSignature(jsonSign);
    return true;
  });

  //Fix checked=checked issue
  $("input[checked]").attr("checked", true).checkboxradio("refresh");

});

function BuildText(bindingsMap, elementsArray) {
  for (var i = 0; i < elementsArray.length; ++i) {
    control = elementsArray[i];
    var binding = control.getAttribute('xd:binding');
    bindingsMap[binding] = control.value;
  }
}

function BuildRadio(bindingMap, elementsArray) {

  for (var i = 0; i < elementsArray.length; ++i) {
    control = elementsArray[i];
    var bindingName = control.getAttribute('xd:binding');
    if (!bindingMap[bindingName]) {
      bindingMap[bindingName] = '';
    }

    if (control.getAttribute('checked')) {
      bindingMap[bindingName] = control.getAttribute('xd:onvalue');
    }
  }
}

function GetValues() {
  var outValues = {};

  var controls = document.querySelectorAll('input[type=text], textarea, input.signature');
  BuildText(outValues, controls);

  controls = document.querySelectorAll('input[type=radio]');
  BuildRadio(outValues, controls);

  return outValues;
}
