'use strict';

require.context('./img', false, /\.(png|jpe?g|svg|ico)$/);
require('./index.html');
require('./index.css');
require('./loading-spinner.css');

var Elm = require('./Main.elm');
var mountNode = document.getElementById('elm-root');
var app = Elm.Main.embed(mountNode);
