#!/usr/bin/env python3
"""
Setup script for Day 1 API Learning
Run this script to install dependencies and start the API server
"""

import subprocess
import sys
import os

def run_command(command, description):
    """Run a command and handle errors"""
    print(f"🔄 {description}...")
    try:
        result = subprocess.run(command, shell=True, check=True, capture_output=True, text=True)
        print(f"✅ {description} completed successfully")
        return True
    except subprocess.CalledProcessError as e:
        print(f"❌ {description} failed:")
        print(f"Error: {e.stderr}")
        return False

def main():
    print("🚀 Setting up Day 1 API Learning Environment")
    print("=" * 50)
    
    # Check if Python is available
    if not run_command("python --version", "Checking Python installation"):
        print("❌ Python is not installed or not in PATH")
        sys.exit(1)
    
    # Install dependencies
    if not run_command("pip install -r requirements.txt", "Installing dependencies"):
        print("❌ Failed to install dependencies")
        sys.exit(1)
    
    print("\n🎉 Setup completed successfully!")
    print("\n📋 Next steps:")
    print("1. Start the API server: python src/main.py")
    print("2. Open http://localhost:8000 in your browser")
    print("3. Explore the interactive docs at http://localhost:8000/docs")
    print("4. Import the Postman collection from Day-1/resources/postman-collection.json")
    
    # Ask if user wants to start the server now
    response = input("\n🤔 Would you like to start the API server now? (y/n): ")
    if response.lower() in ['y', 'yes']:
        print("\n🚀 Starting API server...")
        print("Press Ctrl+C to stop the server")
        os.system("python src/main.py")

if __name__ == "__main__":
    main() 