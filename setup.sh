#!/bin/bash
# Script to setup the project with new features

# Colors for output
GREEN='\033[0;32m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

echo -e "${BLUE}========================================${NC}"
echo -e "${BLUE}Tuan6 Bookstore - Setup Script${NC}"
echo -e "${BLUE}========================================${NC}"

# Check if dotnet is installed
if ! command -v dotnet &> /dev/null; then
    echo "❌ dotnet CLI not found. Please install .NET SDK"
    exit 1
fi

echo -e "\n${BLUE}Step 1: Restore NuGet packages${NC}"
dotnet restore

echo -e "\n${BLUE}Step 2: Create database migration for PaymentTransaction${NC}"
dotnet ef migrations add AddPaymentTransaction

echo -e "\n${BLUE}Step 3: Update database${NC}"
dotnet ef database update

echo -e "\n${GREEN}✅ Setup completed!${NC}"
echo -e "\n${BLUE}Next steps:${NC}"
echo "1. Update appsettings.json with your VNPay/MoMo credentials"
echo "2. Add Chatbot API Key (OpenAI or Gemini)"
echo "3. Run the application: dotnet run"
echo "4. Test at: https://localhost:7075"
echo -e "\n${BLUE}Features added:${NC}"
echo "✨ QR Payment (VNPay & MoMo)"
echo "✨ AI Chatbot"
echo "✨ Payment Transaction Tracking"
