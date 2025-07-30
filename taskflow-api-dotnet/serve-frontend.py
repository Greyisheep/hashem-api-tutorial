#!/usr/bin/env python3
"""
Simple HTTP server for TaskFlow Frontend
Serves the frontend files for testing OAuth and security features
"""

import http.server
import socketserver
import os
import sys
from pathlib import Path

# Change to the frontend directory
frontend_dir = Path(__file__).parent / "frontend"
os.chdir(frontend_dir)

# Configuration
PORT = 3000
HOST = "localhost"

class CustomHTTPRequestHandler(http.server.SimpleHTTPRequestHandler):
    def end_headers(self):
        # Add CORS headers for development
        self.send_header('Access-Control-Allow-Origin', '*')
        self.send_header('Access-Control-Allow-Methods', 'GET, POST, OPTIONS')
        self.send_header('Access-Control-Allow-Headers', 'Content-Type, Authorization')
        super().end_headers()
    
    def do_OPTIONS(self):
        self.send_response(200)
        self.end_headers()

def main():
    try:
        with socketserver.TCPServer((HOST, PORT), CustomHTTPRequestHandler) as httpd:
            print(f"🚀 TaskFlow Frontend Server")
            print(f"📍 Serving at: http://{HOST}:{PORT}")
            print(f"📁 Directory: {frontend_dir.absolute()}")
            print(f"🔗 API URL: http://localhost:7001")
            print()
            print("Press Ctrl+C to stop the server")
            print("=" * 50)
            
            httpd.serve_forever()
    except KeyboardInterrupt:
        print("\n🛑 Server stopped by user")
    except OSError as e:
        if e.errno == 48:  # Address already in use
            print(f"❌ Error: Port {PORT} is already in use!")
            print(f"   Try: lsof -ti:{PORT} | xargs kill -9")
        else:
            print(f"❌ Error: {e}")
        sys.exit(1)

if __name__ == "__main__":
    main() 