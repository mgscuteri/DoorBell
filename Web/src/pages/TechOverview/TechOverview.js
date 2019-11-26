import React, { Component } from 'react';

class TechOverview extends React.Component {  
    constructor(props) {
      super(props);
    }

    render() {
      return (
         <p className="App-intro">
            <h2>
              About
            </h2>
            <p>
             Below is an overview of the technologies being used by this website, and the server it runs on. 
            </p>

            <ul>
              <li>Server</li>
                <ul>
                  <li><a href="https://www.raspberrypi.org/products/raspberry-pi-3-model-b/">Raspberry Pi 3</a> - Hosts this Application, as well as Jenkins </li>
                  <li><a href="https://www.raspberrypi.org/downloads/raspbian/"> Raspbian Stretch Lite </a> - Operating System</li>
                  <li><a href="https://circleci.com/migrate-jenkins-to-circleci/?utm_source=gnb&utm_medium=SEM&utm_campaign=SEM-gnb-400-Eng-ni&utm_content=SEM-gnb-400-Eng-ni-Jenkins&gclid=EAIaIQobChMI8ejf59fV4AIVxkSGCh0zDQ6MEAAYASAAEgLG9fD_BwE"> Jenkins </a> - Integrates with Github to orchestrate continuous integration and deployment</li>
                </ul>
              <li>Back-End</li>
                <ul>
                  <li><a href="https://www.python.org/">Python3</a> - Serves static content and handles rest calls</li>
                  <li><a href="https://aiohttp.readthedocs.io/en/stable/">Aiohttp </a> - used for HTTP handling </li>
                </ul>
              <li> Front-End
                <ul>
                  <li><a href="https://www.npmjs.com/">Node Package Manager </a> - Dependency Management</li> 
                  <li><a href="https://reactjs.org/">React</a> - Javacript Framework</li>
                  <li><a href="https://webpack.js.org/">Webpack</a> - Bundles HTML/Js/CSS and provides development server </li>
                  <li><a href="https://webpack.js.org/">Axios</a> - Package used for making REST calls </li>
                </ul>
              </li>
              <li>Routing/DNS</li>
                <ul>
                    <li><a href="https://www.tp-link.com/us/products/details/cat-9_Archer-C7.html">Tp-Link Archer C7</a> Port Forwarding and VPN Server</li> 
                    <li><a href="https://www.noip.com/">Noip</a> DNS registration and dynamic DNS servicing </li> 
                </ul>
              <li>Source Control</li>
                <ul>
                    <li><a href="https://github.com/mgscuteri/PythonPoweredPi">Github</a></li> 
                </ul>
            </ul>
         </p>
      );
    }
  }

export default TechOverview