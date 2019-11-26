import React, { Component } from 'react';
import ReactDOM from 'react-dom'
import { Route, Link, BrowserRouter as Router } from 'react-router-dom'
import './App.css';

import Navbar from './modules/site/navbar.js'
import Footer from './modules/site/footer.js'



class App extends Component {
  render() {
    return (
      <div>
        <Navbar/>
        <Footer/>
      </div>
    );
  }
}


export default App;
