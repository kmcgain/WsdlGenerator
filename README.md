WsdlGenerator
=============

Parse a Wsdl and provide an output similar to svcutil.exe. The purpose of this tool is to allow customisations to the output that you cannot get with svcutil.

Dependencies:
T4Toolbox : https://t4toolbox.codeplex.com/

Usage:
The code is broken down into two projects. CSGeneration and WsdlGenerator
CSGeneration is a supporting class library that shouldn't need to be modified when creating custom outputs.
WsdlGenerator is the driver project that contains the default generation templates which may be modifed to vary the output.

CustomServiceReferenceGenerator.tt is the starting place for the T4 and has a service reference on line 3. This should be changed to point to your own web service. Once this is set you can run the T4 template via the Build menu -> Transform All T4 Templates.

This will output the transformed service code to Service1.generated.cs