/** @format */

const path = require("path");
require("dotenv").config({ path: path.resolve(__dirname, ".env") });
const CopyWebpackPlugin = require("copy-webpack-plugin");

if (!process.env.VIEWER_OUTPUT_PATH) {
  throw new Error("VIEWER_OUTPUT_PATH is not defined");
}

module.exports = {
  entry: {
    code: "./wwwroot/js/main.js",
    style: "./wwwroot/css/main.scss",
  },
  // plugins: [
  //   new CopyWebpackPlugin({
  //     patterns: [
  //       // {
  //       //   from: 'wwwroot/img',
  //       //   to: path.resolve(__dirname, '../Examples/Immense.RemoteControl.Examples.ServerExample/wwwroot/img'),
  //       // },
  //     ],
  //   }),
  // ],
  // resolve: {
  //   modules: [path.resolve(__dirname, '../node_modules')],
  // },
  module: {
    rules: [
      {
        test: /\.(scss|css)$/i,
        use: ["style-loader", "css-loader", "sass-loader"],
      },
      //     // {
      //     //   test: /\.(woff(2)?|ttf|eot)$/,
      //     //   type: 'asset/resource',
      //     //   generator: {
      //     //     filename: './font-family/Segoe/[name][ext]',
      //     //   },
      //     // },
    ],
  },
  performance: {
    hints: false,
  },
  output: {
    filename: "[name].js",
    path: path.resolve(
      __dirname,
      process.env.VIEWER_OUTPUT_PATH + "/remotecontrol_viewer"
    ),
    clean: true,
  },
};
