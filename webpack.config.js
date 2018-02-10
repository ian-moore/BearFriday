var path = require('path');

module.exports = {
  entry: './src/client/index.js',

  output: {
    path: path.join(__dirname, 'dist'),
    filename: 'app.js'
  },

  module: {
    rules: [
      {
        test: /\.html$/,
        exclude: /node_modules/,
        use: 'file-loader?name=[name].[ext]'
      },
      {
        test: /\.jpe?g$|\.ico$|\.gif$|\.png$|\.svg$|\.woff$|\.ttf$|\.wav$|\.mp3$/,
        loader: 'file-loader?name=img/[name].[ext]'
      },
      {
        test: /\.elm$/,
        exclude: [/elm-stuff/, /node_modules/],
        use: [
          'elm-webpack-loader'
        ]
      },
      {
        test: /\.css$/,
        use: [ 'style-loader', 'css-loader' ]
      }
    ]
  },

  target: 'web',

  devServer: {
    historyApiFallback: true,
    inline: true,
    stats: 'errors-only'
  }
};