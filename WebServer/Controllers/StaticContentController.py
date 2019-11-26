import http.server
import socketserver
import os
import sys
import time
import random
import asyncio
from aiohttp import web

class StaticContentController:
    currentDirectory = ''
    reactBuildLocation = ''
    app = web.Application()

    def __init__(self, app, currentDirectory):
        self.app = app
        self.currentDirectory = currentDirectory
        self.reactBuildLocation = currentDirectory + '/Web/build/'
        app.router.add_route('GET', '/', self.getHomePage)
        app.router.add_route('GET', '/ThemeSongs', self.getHomePage)
        app.router.add_route('GET', '/About', self.getHomePage)
        app.router.add_route('GET', '/service-worker.js', self.getJsServiceWorker)
        app.router.add_route('*', '/static/css/{cssFileName}', self.getCss)
        app.router.add_route('*', '/static/js/{jsFileName}', self.getJs)
        app.router.add_route('*', '/static/media/{mediaFileName}', self.getMedia)

    def getHomePage(self, request):
        return web.FileResponse(self.reactBuildLocation +'index.html')
    def getJsServiceWorker(self, request):
        return web.FileResponse(self.reactBuildLocation +'service-worker.js')
    def getJs(self, request):
        jsFilePath = self.reactBuildLocation + '/static/js/{}'.format(request.match_info['jsFileName'])
        return web.FileResponse(jsFilePath)
    def getCss(self, request):
        cssFilePath = self.reactBuildLocation + '/static/css/{}'.format(request.match_info['cssFileName'])
        return web.FileResponse(cssFilePath)
    def getMedia(self, request):
        mediaFilePath = self.reactBuildLocation + '/static/media/{}'.format(request.match_info['mediaFileName'])
        return web.FileResponse(mediaFilePath)
