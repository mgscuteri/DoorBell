import React, { Component } from 'react';
import piLogo from '../../images/piLogoSmall.png';
import ReactDOM from 'react-dom'
import { Route, Link, BrowserRouter as Router } from 'react-router-dom'
import { Switch } from 'react-router-dom';

import ThemeSongs from '../../pages/ThemeSongs/ThemeSongs.js'
import HomePage from '../../pages/HomePage/HomePage.js'
import TechOverview from '../../pages/TechOverview/TechOverview.js';
import LogIn from '../../pages/LogIn/LogIn.js';


class Navbar extends React.Component {  
    constructor(props) {
      super(props);
    }

    render() {
      return (
          <Router>
            <div>
              <div class="navBarContainer">
                <div class="navbar">
                  <ul class="topnav">
                    <li><Link to="/"><img src={piLogo} alt="logo"/></Link></li>
                    <li class="topnav"><Link to="/">Home</Link></li>
                    <li class="topnav, right"><Link to="/LogIn">Log In</Link></li>
                    <li class="topnav"><Link to="/ThemeSongs">ThemeSongs</Link></li>
                    <li class="topnav"><Link to="/About">About</Link></li>
                  </ul>
                </div>
              </div>
              <div className="appBody">
                <Switch>
                  <Route exact path="/" component={HomePage} />
                  <Route path="/LogIn" component={LogIn} />
                  <Route path="/ThemeSongs" component={ThemeSongs} />
                  <Route path="/About" component={TechOverview} />
                  <Route render={ () => <h1>404 Error</h1> } />
                </Switch>
              </div>
            </div>
          </Router>
      );
    }
  }

export default Navbar