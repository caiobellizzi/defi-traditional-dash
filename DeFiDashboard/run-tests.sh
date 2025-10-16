#!/bin/bash

# DeFi Dashboard - Test Runner Script
# This script runs all tests (unit + integration) and generates coverage reports

set -e  # Exit on error

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

echo "================================================"
echo "  DeFi Dashboard - Test Runner"
echo "================================================"
echo ""

# Function to print colored output
print_success() {
    echo -e "${GREEN}✓ $1${NC}"
}

print_error() {
    echo -e "${RED}✗ $1${NC}"
}

print_info() {
    echo -e "${YELLOW}ℹ $1${NC}"
}

# Check if .NET SDK is installed
if ! command -v dotnet &> /dev/null; then
    print_error ".NET SDK not found. Please install .NET 9 SDK."
    exit 1
fi

print_info "Using .NET version: $(dotnet --version)"
echo ""

# Navigate to project root
cd "$(dirname "$0")"
PROJECT_ROOT=$(pwd)

# ================================================
# 1. Run Backend Unit Tests
# ================================================
echo "================================================"
echo "  Running Backend Unit Tests"
echo "================================================"
echo ""

cd "$PROJECT_ROOT/tests/ApiService.Tests"

print_info "Running unit tests with coverage..."
dotnet test \
    --configuration Release \
    --logger "console;verbosity=detailed" \
    --collect:"XPlat Code Coverage" \
    --results-directory:"$PROJECT_ROOT/test-results/unit"

if [ $? -eq 0 ]; then
    print_success "Unit tests passed!"
else
    print_error "Unit tests failed!"
    exit 1
fi

echo ""

# ================================================
# 2. Run Backend Integration Tests
# ================================================
echo "================================================"
echo "  Running Backend Integration Tests"
echo "================================================"
echo ""

cd "$PROJECT_ROOT/tests/ApiService.IntegrationTests"

print_info "Starting Testcontainers for PostgreSQL..."
print_info "Running integration tests with coverage..."
dotnet test \
    --configuration Release \
    --logger "console;verbosity=detailed" \
    --collect:"XPlat Code Coverage" \
    --results-directory:"$PROJECT_ROOT/test-results/integration"

if [ $? -eq 0 ]; then
    print_success "Integration tests passed!"
else
    print_error "Integration tests failed!"
    exit 1
fi

echo ""

# ================================================
# 3. Generate Coverage Report
# ================================================
echo "================================================"
echo "  Generating Coverage Report"
echo "================================================"
echo ""

# Check if reportgenerator is installed
if ! command -v reportgenerator &> /dev/null; then
    print_info "Installing ReportGenerator tool..."
    dotnet tool install -g dotnet-reportgenerator-globaltool
fi

print_info "Generating HTML coverage report..."
reportgenerator \
    -reports:"$PROJECT_ROOT/test-results/**/coverage.cobertura.xml" \
    -targetdir:"$PROJECT_ROOT/test-results/coverage-report" \
    -reporttypes:"Html;TextSummary"

if [ $? -eq 0 ]; then
    print_success "Coverage report generated!"
    echo ""
    print_info "View coverage report at: $PROJECT_ROOT/test-results/coverage-report/index.html"
else
    print_error "Failed to generate coverage report!"
fi

echo ""

# ================================================
# 4. Display Coverage Summary
# ================================================
if [ -f "$PROJECT_ROOT/test-results/coverage-report/Summary.txt" ]; then
    echo "================================================"
    echo "  Coverage Summary"
    echo "================================================"
    echo ""
    cat "$PROJECT_ROOT/test-results/coverage-report/Summary.txt"
    echo ""
fi

# ================================================
# 5. Run Frontend Tests (Optional)
# ================================================
if [ -d "$PROJECT_ROOT/frontend" ]; then
    echo "================================================"
    echo "  Running Frontend Tests"
    echo "================================================"
    echo ""

    cd "$PROJECT_ROOT/frontend"

    # Check if Node.js is installed
    if ! command -v npm &> /dev/null; then
        print_error "Node.js/npm not found. Skipping frontend tests."
    else
        print_info "Installing frontend dependencies..."
        npm install --silent

        # Run E2E tests if playwright is configured
        if [ -f "playwright.config.ts" ]; then
            print_info "Running Playwright E2E tests..."
            npm run test:e2e

            if [ $? -eq 0 ]; then
                print_success "Frontend E2E tests passed!"
            else
                print_error "Frontend E2E tests failed!"
            fi
        else
            print_info "Playwright not configured. Skipping E2E tests."
        fi
    fi

    echo ""
fi

# ================================================
# Summary
# ================================================
echo "================================================"
echo "  Test Summary"
echo "================================================"
echo ""
print_success "All tests completed successfully!"
echo ""
print_info "Test Results Location: $PROJECT_ROOT/test-results/"
print_info "Coverage Report: $PROJECT_ROOT/test-results/coverage-report/index.html"
echo ""

# Open coverage report in browser (optional)
if command -v open &> /dev/null; then
    read -p "Open coverage report in browser? (y/n) " -n 1 -r
    echo
    if [[ $REPLY =~ ^[Yy]$ ]]; then
        open "$PROJECT_ROOT/test-results/coverage-report/index.html"
    fi
fi

echo "================================================"
exit 0
