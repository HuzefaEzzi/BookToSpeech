
# Project Name: EpubToAudioConverter

## Overview
The EpubToAudioConverter project is designed to convert EPUB documents into audio files using the Azure Text to Speech (TTS) API. It parses EPUB documents, extracts text chunks, and sends them to the Azure TTS API to generate corresponding audio files. The project utilizes Azure Storage for managing containers and queues to streamline the conversion process.

## Dependencies
- Azure Services
  - Storage
  - Container: samples-workitems - Used to store EPUB documents and intermediate text files.
- Queue
  - Queue: sample-worker - Used to manage the workflow and communication between different components of the project.
- Azure Text to Speech (TTS) API
  - The project relies on the Azure Text to Speech API for converting text chunks into audio files.

Ensure that you have an active Azure subscription.
Set up the required Azure Storage container (samples-workitems) and queue (sample-worker).
Obtain the necessary Azure Text to Speech API credentials.
