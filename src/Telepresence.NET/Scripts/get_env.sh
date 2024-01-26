#!/bin/bash
for key in $(env | cut -d= -f1); do
    echo "Key: $key"
    echo "Value: ${!key}"
done