#!/bin/bash

# Prompt the user for the IP address of the EC2 instance
# read -p "Enter EC2 instance IP address: " EC2_IP

# Prompt the user for the location of the bucstop.pem key
# read -p "Enter the path to your bucstop.pem key: " PEM_PATH

# SSH into the EC2 instance
# echo "Connecting to EC2 instance at $EC2_IP..."
# ssh -i "$PEM_PATH" ec2-user@"$EC2_IP" << "EOF"


# Change to the BucStop folder
echo "Navigating to BucStop folder..."
cd /home/ec2-user/Team-3-BucStop-sprint_8/BucStop || { echo "BucStop folder not found!"; exit 1; }

# Ensure dotnet 6.0 is installed
echo "Installing .NET SDK 6.0..."
sudo yum install -y dotnet-sdk-6.0

# Install Docker (not sure if necessary, but let's do it)
echo "Installing Docker..."
sudo yum install -y docker

# Start and enable Docker
echo "Starting Docker..."
sudo systemctl start docker
sudo systemctl enable docker

# Build and run the Docker container for BucStop
echo "Building Docker image for BucStop..."
docker build -t bucstop-app .

echo "Running Docker container..."
sudo docker run -d -p 80:80 bucstop-app

# Run .NET tasks
echo "Restoring .NET dependencies..."
sudo dotnet restore

echo "Building .NET project..."
sudo dotnet build

echo "Running .NET project..."
sudo dotnet run

# Provide the URL to access the application
echo "The application should now be accessible at http://$EC2_IP:8000/"

echo "Script finished!"
