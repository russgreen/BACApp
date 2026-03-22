#!/bin/bash

APP_PATH="$1"
SIGN_IDENTITY="$2"

echo "Resigning frameworks in $APP_PATH with identity: $SIGN_IDENTITY"

find "$APP_PATH/Frameworks" -type d -name "*.framework" | while read framework; do
    echo "Signing $framework"
    codesign --force --deep --sign "$SIGN_IDENTITY" "$framework"
done