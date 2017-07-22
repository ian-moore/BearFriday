var path = require("path"),
  CopyWebpackPlugin = require('copy-webpack-plugin');

module.exports = {
  entry: {
    friday: [
      './src/Friday.js'
    ]
  },

  output: {
    path: path.resolve(__dirname + '/dist'),
    filename: '[name].js',
  },

  module: {
    rules: [
      {
        test:    /\.elm$/,
        exclude: [/elm-stuff/, /node_modules/],
        loader:  'elm-webpack-loader?verbose=true&warn=true',
      }
    ],

    noParse: /\.elm$/,
  },

  plugins: [
    new CopyWebpackPlugin([
      { context: 'public/', from: '**/*', }
    ])    
  ],

  devServer: {
    inline: true,
    stats: { colors: true },
  },

};