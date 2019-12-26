rmdir /s /q package
mkdir package
mkdir package\managed
mkdir package\native-src
copy managed package\managed
copy binding.gyp package
copy package.json package
copy native-src package\native-src
copy dist package
