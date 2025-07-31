#!/usr/bin/env python3
"""
Simple HTTP server to serve the Squad.co payment demo frontend
"""

import http.server
import socketserver
import os
import sys
from pathlib import Path

def main():
    # Get the directory where this script is located
    script_dir = Path(__file__).parent
    frontend_dir = script_dir / "frontend"
    
    if not frontend_dir.exists():
        print(f"Error: Frontend directory not found at {frontend_dir}")
        sys.exit(1)
    
    # Change to the frontend directory
    os.chdir(frontend_dir)
    
    # Set up the server
    PORT = 8081
    
    class MyHTTPRequestHandler(http.server.SimpleHTTPRequestHandler):
        def end_headers(self):
            # Add CORS headers for development
            self.send_header('Access-Control-Allow-Origin', '*')
            self.send_header('Access-Control-Allow-Methods', 'GET, POST, PUT, DELETE, OPTIONS')
            self.send_header('Access-Control-Allow-Headers', 'Content-Type, Authorization')
            super().end_headers()
        
        def do_OPTIONS(self):
            self.send_response(200)
            self.end_headers()
    
    try:
        with socketserver.TCPServer(("", PORT), MyHTTPRequestHandler) as httpd:
            print(f"🚀 Serving Squad.co Payment Demo at http://localhost:{PORT}")
            print(f"📁 Serving files from: {frontend_dir}")
            print(f"🔗 API Base URL: http://localhost:5000")
            print(f"📧 Notification Email: greyisheep@gmail.com")
            print("\n✨ Features:")
            print("   • Payment Modal Integration")
            print("   • Direct API Integration (Card, Bank, USSD)")
            print("   • Webhook Processing")
            print("   • Redirect URL Handling")
            print("   • Email Notifications")
            print("   • Transaction Verification")
            print("\n🛑 Press Ctrl+C to stop the server")
            httpd.serve_forever()
    except KeyboardInterrupt:
        print("\n👋 Server stopped by user")
    except OSError as e:
        if e.errno == 48:  # Address already in use
            print(f"❌ Port {PORT} is already in use. Please try a different port or stop the existing server.")
        else:
            print(f"❌ Error starting server: {e}")
        sys.exit(1)

if __name__ == "__main__":
    main() 