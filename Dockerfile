FROM openjdk:8-jre-alpine

WORKDIR /app
COPY target/*with*.jar /app/app.jar

CMD ["/usr/bin/java", "-jar", "app.jar"]