pipeline {
    agent any
    stages {
        stage('Test Build Docker Images') {
            steps {
                echo 'Testing Docker build for backend service without running containers'
                script {
                    def buildResult = sh(script: "docker-compose -f docker-compose.yml build backend", returnStatus: true)
                    if (buildResult != 0) {
                        error("Docker build failed! Stopping pipeline.")
                    } else {
                        echo "Docker build successful. Proceeding with the pipeline..."
                    }
                }
            }
        }
        stage('Build and Run Backend') {
            steps {
                echo 'Removing old backend container (if exists)'
                sh 'docker rm -f backend || true'
                echo 'Building and running backend container'
                sh 'docker-compose -f "docker-compose.yml" up -d --build backend'
            }
        }
        stage('Cleaning up') {
            steps {
                echo 'Cleaning up unused Docker resources'
                sh 'docker system prune -f'
                sh 'docker ps -a'
            }
        }
    }
    post {
        always {
            echo 'Cleaning workspace'
            cleanWs()
        }
    }
}
