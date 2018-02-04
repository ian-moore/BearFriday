'use strict';

require('./index.html');

var Elm = require('./Main.elm');
var mountNode = document.getElementById('elm-root');
var app = Elm.Main.embed(mountNode);
