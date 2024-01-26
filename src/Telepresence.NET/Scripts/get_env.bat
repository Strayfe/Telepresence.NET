@echo off
for /F "tokens=1,2*" %%a in ('set') do (
    echo Key: %%a
    echo Value: %%b
)