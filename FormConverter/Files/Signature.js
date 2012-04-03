var clicked;

function Signature() {

  this.initSignature = function (parent) {
    var me = this;

    this.clicked = '';
    this.canvas = '';
    this.ctx = '';
    this.coords = '';
    this.offsetX = '';
    this.offsetY = '';
    this.oldX = '';
    this.oldY = '';
    this.id = 'canvas_signature_id';

    var canvas = document.createElement('canvas');
    canvas.width = '400';
    canvas.id = 'canvas_' + me.id;
    canvas.height = '175';
    parent.appendChild(canvas);

    this.emptySignature = '';
  };

  this.getSignature = function () {
    return this.canvas.toDataURL("image/png");
  };

  this.isSigned = function()
  {
    return this.getSignature() != this.emptySignature;
  };

  this.setSignature = function (signature) {
    var me = this;
    var imager = new Image();
    imager.onload = function () {
      me.ctx.drawImage(imager, 0, 0);
    };
    imager.src = signature;
  };

  this.clearSignature = function () {
    // drawing the background
    this.ctx.clearRect(0, 0, 400, 175);
    this.ctx.beginPath();
    this.ctx.strokeStyle = "rgb(0,0,0)";
    this.ctx.moveTo(0, 150);
    this.ctx.lineTo(400, 150);
    this.ctx.stroke();
    this.ctx.font = "36pt Arial";
    this.ctx.fillStyle = "rgb(180,33,33)";
    this.ctx.fillText("X", 10, 150);
  };

  this.setupCanvas = function () {
    var me = this;
    this.canvas = document.getElementById('canvas_' + this.id);

    this.ctx = this.canvas.getContext("2d");
    this.coords = this.getCumulativeOffset(this.canvas);
    this.offsetX = this.coords.x;
    this.offsetY = this.coords.y;
    this.clearSignature();

    this.emptySignature = this.getSignature();

    this.canvas.ontouchmove = function (e) {
      var x, y, i;
      e.preventDefault();
      for (i = 0; i < e.targetTouches.length; i++) {
        x = e.targetTouches[i].clientX - me.offsetX;
        y = e.targetTouches[i].clientY - me.offsetY;
        me.drawCircle(x, y);
      }
    };

    this.canvas.onmousemove = function (e) {
      e.preventDefault();
      var x = e.offsetX,
        y = e.offsetY;
      if (me.clicked) me.drawCircle(x, y);
    };

    this.canvas.ontouchstart = function (e) {
      e.preventDefault();
      me.coords = me.getCumulativeOffset(me.canvas);
      me.offsetX = me.coords.x;
      me.offsetY = me.coords.y;
    };

    this.canvas.ontouchend = function (e) {
      e.preventDefault();
      me.oldX = me.oldY = me.clicked = false;
      clicked = me.clicked;
    };
    this.canvas.onmousedown = function (e) {
      e.preventDefault();
      me.clicked = true;
      clicked = me.clicked;
    };
    this.canvas.onmouseup = function (e) {
      e.preventDefault();
      me.oldX = me.oldY = me.clicked = false;
      clicked = me.clicked;
    };

    this.canvas.onmouseover = function (e) {
      me.clicked = clicked;
      if (me.clicked) {
        me.oldX = e.offsetX;
        me.oldY = e.offsetY;
      }
      else {
        me.oldX = me.oldY = false;
      }
    };

  };

  this.drawCircle = function (x, y) {
    this.ctx.strokeStyle = "rgb(55,55,255)";
    this.ctx.beginPath();
    if (this.oldX && this.oldY) {
      this.ctx.moveTo(this.oldX, this.oldY);
      this.ctx.lineTo(x, y);
      this.ctx.stroke();
      this.ctx.closePath();
    }
    this.oldX = x;
    this.oldY = y;
  };

  this.getCumulativeOffset = function (obj) {
    var left = 0,
      top = 0;

    if (obj.offsetParent) {
      do {
        left += obj.offsetLeft;
        top += obj.offsetTop;
      } while (obj = obj.offsetParent);
    }
    return {x:left, y:top};
  };
}

window.onmousedown = function () {
  clicked = true;
};

window.onmouseup = function (e) {
  clicked = false;
};

window.ontouchend = function () {
  clicked = false;
};


