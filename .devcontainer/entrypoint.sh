#!/bin/bash

# SIGTERM-handler this funciton will be executed when the container receives the SIGTERM signal (when stopping)
term_handler(){
   echo "***Stopping"
   podman stop --all
   exit 0
}

# Setup signal handlers
trap 'term_handler' SIGTERM