
All projects using DotWeb assembly also depend on DevExpress assemblies.
In the development environment, you can just install DevExpress from the installation media.
But in production, such installation could be prohibited by existing policy.
The solution is to include all required DevExpress assemblies here and configure them as ContentWithTargetPath.
Build process will automatically copy all assemblies contaied inside this folder
to output directory.