'use strict';

require('./index.html');

var Elm = require('./Main.elm');
var mountNode = document.getElementById('elmRoot');
var app = Elm.Main.embed(mountNode);
