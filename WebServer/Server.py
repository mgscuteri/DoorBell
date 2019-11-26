import json
import _thread
import http.server
import socketserver
import os
import os.path
import sys
import time
import random
import asyncio
from aiohttp import web
#HTTP controllers
from Controllers.StaticContentController import StaticContentController
from Controllers.NumbersController import NumbersController
from Controllers.DataController import DataController

def LaunchServer():
    print("Launching REST Server")
    currentDirectory = os.getcwd()
    currentDirectory = os.path.abspath(os.path.join(currentDirectory, os.pardir))    
    print(currentDirectory)
    app = web.Application()
    #Instantiate HTTP Controllers 
    app_server = StaticContentController(app, currentDirectory)
    app = app_server.app
    app_server = NumbersController(app, currentDirectory)
    app = app_server.app
    app_server = DataController(app, currentDirectory)
    app = app_server.app
    #Launch Server
    loop = asyncio.get_event_loop()
    handler = app.make_handler()
    f = loop.create_server(handler, '0.0.0.0', 8080)
    srv = loop.run_until_complete(f)
    print('serving on', srv.sockets[0].getsockname())
    try:
        loop.run_forever()
    except KeyboardInterrupt:
        pass
    finally:
        srv.close()
        loop.run_until_complete(srv.wait_closed())
        loop.run_until_complete(app.finish())
    loop.close()

if __name__ == '__main__':
    LaunchServer()
    


