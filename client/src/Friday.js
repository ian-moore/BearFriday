'use strict';

var Elm = require('./Friday.elm'),
    mountNode = document.getElementById('app'),
    app = Elm.Friday.embed(mountNode);