pipeline {
  agent {
    docker {
      label 'docker'
      image 'microsoft/dotnet'
    }
  }
  stages {
    stage('Build') {
      steps {
        sh 'git clean -fdx'
        sh 'dotnet msbuild /t:restore /p:RestoreSources="https://api.nuget.org/v3/index.json;https://api.bintray.com/nuget/fint/nuget" Fint.Sse.sln'
        sh 'dotnet msbuild /t:build /p:Configuration=Release Fint.Sse.sln'
        sh 'dotnet msbuild /t:pack /p:Configuration=Release Fint.Sse.sln'
        sh 'dotnet test Fint.Sse.Tests'
        stash includes: "**/Release/*.nupkg", name: 'libs'
      }
    }
    stage('Deploy') {
      environment {
        BINTRAY = credentials('fint-bintray')
      }
      when {
        branch 'master'
      }
      steps {
        deleteDir()
        unstash 'libs'
        archiveArtifacts '**/*.nupkg'
        //sh "dotnet nuget push Fint.Sse/bin/Release/Fint.Sse.*.nupkg -k ${BINTRAY} -s https://api.bintray.com/nuget/fint/nuget"
      }
    }
  }
}
