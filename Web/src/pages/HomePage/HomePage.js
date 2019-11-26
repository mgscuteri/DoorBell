import React, { Component } from 'react';

class HomePage extends React.Component {  
    constructor(props) {
      super(props);
    }

    render() {
      return (
        <div>
          <h2>
            Welcome to the SmartBell intelligent DoorBell. 
          </h2>
          <p>
            You can use this website to associate your phone with a themesong, and whenever your phone connects to this network, your themesong will be played!
          </p>          
          <br/>
        </div>
      );
    }
  }

export default HomePage
