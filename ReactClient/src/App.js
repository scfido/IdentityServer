import React, { Component } from 'react';
import logo from './logo.svg';
import './App.css';
import Oidc from "oidc-client";
import "babel-polyfill";

const config = {
  authority: "http://localhost:5000",
  client_id: "js",
  redirect_uri: "http://localhost:3000/callback.html",
  response_type: "id_token token",
  scope: "openid profile api1",
  post_logout_redirect_uri: "http://localhost:3000/index.html",
};

const mgr = new Oidc.UserManager(config);

class App extends Component {
  constructor() {
    super();
    this.state = { loginStatus: "未登录" };
  }

  componentDidMount() {
    let self = this;
    mgr.getUser().then(function (user) {
      if (user) {
        self.setState({
          loginStatus: "User logged in",
          userProfile: user.profile
        });
      }
      else {
        self.setState({
          loginStatus: "User not logged in",
          userProfile: null
        });
      }
    });
  }

  login() {
    mgr.signinRedirect();
  }

  api() {
    mgr.getUser().then(function (user) {
      var url = "http://localhost:5001/api/identity";

      var xhr = new XMLHttpRequest();
      xhr.open("GET", url);
      xhr.onload = function () {
        console.log(xhr.status, JSON.parse(xhr.responseText));
      }
      xhr.setRequestHeader("Authorization", "Bearer " + user.access_token);
      xhr.send();
    });
  }

  logout() {
    mgr.signoutRedirect();
  }

  render() {
    return (
      <div className="App">
        <button id="login" onClick={this.login.bind(this)}>Login</button>
        <button id="api" onClick={this.api.bind(this)}>Call API</button>
        <button id="logout" onClick={this.logout.bind(this)}>Logout</button>

        <pre id="results">{this.state.loginStatus}
        </pre>
      </div>
    );
  }
}

export default App;
