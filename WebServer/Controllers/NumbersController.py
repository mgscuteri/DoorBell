import http.server
import socketserver
import os
import sys
import time
import random
import asyncio
from aiohttp import web

class NumbersController:
    currentDirectory = os.getcwd()
    controllerPrefix = '/numbers'
    app = web.Application()

    def __init__(self, app, currentDirectory):
        self.app = app
        self.currentDirectory = currentDirectory
        app.router.add_route('GET', self.controllerPrefix + '/rand', self.getRandomNumber)
        app.router.add_route('GET', self.controllerPrefix + '/add/{intA}/{intB}', self.addNumbers)
        app.router.add_route('GET', self.controllerPrefix + '/subtract/{intA}/{intB}', self.subtractNumbers)
        app.router.add_route('GET', self.controllerPrefix + '/multiply/{intA}/{intB}', self.multiplyNumbers)
        app.router.add_route('GET', self.controllerPrefix + '/divide/{intA}/{intB}', self.divideNumbers)
        
    def getRandomNumber(self, request):
        return web.json_response(random.randint(1,100))
    def addNumbers(self, request):
        intA = int(request.match_info['intA'])
        intB = int(request.match_info['intB'])
        abSum = intA + intB 
        return web.json_response(abSum)
    def subtractNumbers(self, request):
        intA = int(request.match_info['intA'])
        intB = int(request.match_info['intB'])
        abSum = intA - intB 
        return web.json_response(abSum)
    def multiplyNumbers(self, request):
        intA = int(request.match_info['intA'])
        intB = int(request.match_info['intB'])
        abResult = intA * intB 
        return web.json_response(abResult)
    def divideNumbers(self, request):
        intA = int(request.match_info['intA'])
        intB = int(request.match_info['intB'])
        abResult = intA / intB 
        return web.json_response(abResult)
