#!/bin/bash

echo "========================================"
echo "TaskFlow Frontend - Security Testing"
echo "========================================"
echo

# Check if Python is available
if ! command -v python &> /dev/null; then
    echo "ERROR: Python is not installed!"
    echo "Please install Python and try again."
    exit 1
fi

echo "Starting frontend server..."
echo "Frontend will be available at: http://localhost:3000"
echo "Press Ctrl+C to stop the server"
echo

# Start the frontend server
python serve-frontend.py 