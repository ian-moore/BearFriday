'use strict';

require('./index.html');
require('./index.css');

var Elm = require('./Main.elm');
var mountNode = document.getElementById('elm-root');
var app = Elm.Main.embed(mountNode);
